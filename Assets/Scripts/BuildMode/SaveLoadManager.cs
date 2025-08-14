// ============================================================================
// Scripts/BuildMode/SaveLoadManager.cs — JSON save/load for build
// ============================================================================
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    public VehicleAssembler assembler;
    public BlockRegistry registry;
    public string fileName = "vehicle.json";

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S)) Save();
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.L)) Load();
    }

    public void Save()
    {
        var data = new VehicleData { blocks = new List<BlockInstance>() };
        foreach (var go in assembler.runtimeBlocks)
        {
            if (!go) continue;
            var dmg = go.GetComponent<DamageableBlock>();
            if (dmg == null) continue;

            var inst = new BlockInstance
            {
                defId = dmg.Def.id,
                gridPos = Vector3Int.RoundToInt(go.transform.localPosition / GridUtil.CellSize),
                rotation = go.transform.localRotation,
                currentHP = dmg.CurrentHP
            };
            data.blocks.Add(inst);
        }
        var json = JsonUtility.ToJson(data, true);
        var path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(path, json);
        Debug.Log($"Saved to {path}");
    }

    public void Load()
    {
        var path = Path.Combine(Application.persistentDataPath, fileName);
        if (!File.Exists(path)) { Debug.LogWarning("No save found"); return; }
        var json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<VehicleData>(json);
        assembler.BuildFromData(data);
        Debug.Log($"Loaded from {path}");
    }
}
