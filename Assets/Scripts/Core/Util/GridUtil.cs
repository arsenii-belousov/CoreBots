// ============================================================================
// Scripts/BuildMode/GridUtil.cs — grid snapping and cell indexing
// ============================================================================
using UnityEngine;

public static class GridUtil
{
    public const float CellSize = 1f;

    static Vector3 GridOrigin = new Vector3(0.0f, 1.5f, 0.0f); // world-space origin (corner of cell 0,0,0)

    public static Vector3 GetGridOrigin()
    {
        return GridOrigin;
    }

    // Snap to nearest cell center (NOT corner). Uses rounding so small negatives like -0.2 go to 0 cell.
    public static Vector3 Snap(Vector3 worldPos)
    {
        float cx = Mathf.Round((worldPos.x - GridOrigin.x) / CellSize);
        float cy = Mathf.Round((worldPos.y - GridOrigin.y) / CellSize);
        float cz = Mathf.Round((worldPos.z - GridOrigin.z) / CellSize);
        return new Vector3(
            cx * CellSize + GridOrigin.x,
            cy * CellSize + GridOrigin.y,
            cz * CellSize + GridOrigin.z
        );
    }

    // Convert world position to integer grid coordinates (cell index)
    public static Vector3Int WorldToGrid(Vector3 worldPos)
    {
        // Center-based indexing: cell i covers [i-0.5, i+0.5). This makes -0.2 map to 0 when CellSize=1.
        Vector3 local = worldPos - GridOrigin;
        return new Vector3Int(
            Mathf.RoundToInt(local.x / CellSize),
            Mathf.RoundToInt(local.y / CellSize),
            Mathf.RoundToInt(local.z / CellSize)
        );
    }

    // Convert grid coordinates (cell index) to world center position of that cell
    public static Vector3 GridToWorldCenter(Vector3Int gridPos)
    {
        // If gridPos indexes cell centers directly (due to rounding indexing), we return that center.
        return new Vector3(
            gridPos.x * CellSize + GridOrigin.x,
            gridPos.y * CellSize + GridOrigin.y,
            gridPos.z * CellSize + GridOrigin.z
        );
    }

    // Given a min (corner) grid cell and size in cells, return the world center of the block
    public static Vector3 BlockCenterFromMin(Vector3Int minCell, Vector3Int size)
    {
    // With center-based indexing, the integer cell coordinate already denotes a center.
    // For multi-size using min corner (corner in center-index space): minCornerCenter = (min + (size-1)/2)
    Vector3 centerCell = (Vector3)minCell + (Vector3)(size - Vector3Int.one) * 0.5f;
        return new Vector3(
            centerCell.x * CellSize + GridOrigin.x,
            centerCell.y * CellSize + GridOrigin.y,
            centerCell.z * CellSize + GridOrigin.z
        );
    }

    // Compute the world center from a block's min cell without re-flooring fractional center
    public static Vector3 PreciseBlockCenterFromMin(Vector3Int minCell, Vector3Int size)
    {
        return BlockCenterFromMin(minCell, size);
    }

    // Extrude a grid cell center along a normal by half the block size (in cells);
    // normal must be axis-aligned.
    public static Vector3 ExtrudeCenter(Vector3 snappedCenter, Vector3 normal, Vector3Int rotatedSize)
    {
        normal = normal.normalized;
        float half = 0f;
        if (Mathf.Abs(normal.x) > 0.5f) half = rotatedSize.x * 0.5f * CellSize;
        else if (Mathf.Abs(normal.y) > 0.5f) half = rotatedSize.y * 0.5f * CellSize;
        else half = rotatedSize.z * 0.5f * CellSize;
        return snappedCenter + normal * half;
    }
}