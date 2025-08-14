// ============================================================================
// Scripts/Vehicle/DamageableBlock.cs — block damage and destruction
// ============================================================================
using UnityEngine;

public class DamageableBlock : MonoBehaviour
{
    public BlockDef Def { get; private set; }
    public int CurrentHP { get; private set; }

    VehicleAssembler owner;

    public void Initialize(VehicleAssembler owner, BlockDef def, int hp)
    {
        this.owner = owner;
        this.Def = def;
        this.CurrentHP = hp;
    }

    public void ApplyDamage(int amount)
    {
        CurrentHP -= amount;
        if (CurrentHP <= 0)
        {
            // Detach block: create a shard with Rigidbody and destroy later
            var shard = gameObject;
            shard.transform.SetParent(null, true);
            var rb = shard.GetComponent<Rigidbody>();
            if (!rb) rb = shard.AddComponent<Rigidbody>();
            rb.mass = Mathf.Max(0.1f, Def.mass * 0.8f);
            owner.OnBlockDestroyed(this);
            Destroy(shard, 5f);
        }
    }
}