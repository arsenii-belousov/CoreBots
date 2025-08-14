// ============================================================================
// Scripts/BuildMode/GridUtil.cs — grid snapping and cell indexing
// ============================================================================
using UnityEngine;

public static class GridUtil
{
    public const float CellSize = 1f;

    public static Vector3 Snap(Vector3 worldPos)
    {
        return new Vector3(
            Mathf.Round(worldPos.x / CellSize) * CellSize,
            Mathf.Round(worldPos.y / CellSize) * CellSize,
            Mathf.Round(worldPos.z / CellSize) * CellSize
        );
    }
}