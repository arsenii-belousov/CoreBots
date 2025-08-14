// ============================================================================
// Scripts/Vehicle/VehicleAssembler.cs — assemble robot from VehicleData
// ============================================================================
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleAssembler : MonoBehaviour
{
    public BlockRegistry registry;

    [Header("Runtime State (read-only)")]
    public VehicleData current;
    public List<GameObject> runtimeBlocks = new();

    [Header("Aggregates")] // total stats
    public int totalHP;
    public int totalEnergyCapacity; // total battery provided
    public int totalEnergyLoad;     // total modules consumption

    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (registry != null) registry.Init();
    }

    public void BuildFromData(VehicleData data)
    {
        ClearRuntime();
        current = data;

        foreach (var bi in data.blocks)
        {
            var def = registry.GetById(bi.defId);
            if (def == null || def.prefab == null) continue;

            var goVar = Instantiate(def.prefab, transform);
            goVar.transform.localPosition = GridUtil.CellSize * (Vector3)bi.gridPos;
            goVar.transform.localRotation = bi.rotation;

            // physics: ensure block has collider, but Rigidbody only on root
            var rbChild = goVar.GetComponent<Rigidbody>();
            if (rbChild) Destroy(rbChild);

            var dmg = goVar.GetComponent<DamageableBlock>();
            if (!dmg)
            {
                dmg = goVar.AddComponent<DamageableBlock>();
            }
            dmg.Initialize(this, def, bi.currentHP > 0 ? bi.currentHP : def.blockHP);

            runtimeBlocks.Add(goVar);
        }

        RecalculateAggregates();
        RecalculatePhysicsMass();
    }

    public void RecalculateAggregates()
    {
        totalHP = 0; totalEnergyCapacity = 0; totalEnergyLoad = 0;
        foreach (var go in runtimeBlocks)
        {
            var dmg = go.GetComponent<DamageableBlock>();
            if (!dmg) continue;
            totalHP += Mathf.Max(0, dmg.CurrentHP);
            totalEnergyCapacity += dmg.Def.energyProvide;
            totalEnergyLoad += dmg.Def.energyConsume;
        }
    }

    public int GetNetEnergy() => totalEnergyCapacity - totalEnergyLoad;

    void RecalculatePhysicsMass()
    {
        float mass = 0f;
        foreach (var go in runtimeBlocks)
        {
            var dmg = go.GetComponent<DamageableBlock>();
            if (dmg != null) mass += Mathf.Max(0f, dmg.Def.mass);
        }
        rb.mass = Mathf.Max(1f, mass);
    }

    public void OnBlockDestroyed(DamageableBlock block)
    {
        runtimeBlocks.Remove(block.gameObject);
        Destroy(block.gameObject);
        RecalculateAggregates();
        RecalculatePhysicsMass();
    }

    public void ClearRuntime()
    {
        for (int i = runtimeBlocks.Count - 1; i >= 0; i--)
        {
            if (runtimeBlocks[i]) DestroyImmediate(runtimeBlocks[i]);
        }
        runtimeBlocks.Clear();
    }
}
