// ============================================================================
// Scripts/Vehicle/BlockInstance.cs — serializable block representation in build
// ============================================================================
using UnityEngine;

[System.Serializable]
public struct BlueprintBlock
{
    public BlockDef def; // reference to the block definition
    public Vector3Int gridPos;   // position in grid cells relative to Core
    public Quaternion rotation;  // 90° step rotations

}