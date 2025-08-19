// ============================================================================
// Scripts/Blocks/BlockRegistry.cs — registry of available blocks (for inventory)
// ============================================================================
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Robolike/BlockRegistry", fileName = "BlockRegistry")]
public class BlockRegistry : ScriptableObject
{
    public List<BlockDef> blocks = new();

    private Dictionary<string, BlockDef> _byId;

    public void Init()
    {
        _byId = new Dictionary<string, BlockDef>();
        foreach (var b in blocks)
        {
            if (!string.IsNullOrEmpty(b.id) && !_byId.ContainsKey(b.id))
                _byId.Add(b.id, b);
        }
    }

    public BlockDef GetById(string id)
    {
        if (_byId == null) Init();
        return _byId.TryGetValue(id, out var def) ? def : null;
    }

    public BlockDef GetByIndex(int idx)
    {
        if (idx < 0 || idx >= blocks.Count) return null;
        return blocks[idx];
    }

    public int Count => blocks.Count;
}