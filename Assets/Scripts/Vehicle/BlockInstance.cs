// ============================================================================
// Scripts/Vehicle/BlockInstance.cs — serializable block representation in build
// ============================================================================
using UnityEngine;

[System.Serializable]
public struct BlockInstance
{
    public string defId;
    public Vector3Int gridPos;   // position in grid cells relative to Core
    public Quaternion rotation;  // 90° step rotations
    public int currentHP;        // for saves
}