// ============================================================================
// Scripts/BuildMode/BuildCursor.cs — aiming/preview/placement/removal
// ============================================================================
using UnityEngine;

public class PlacementController : MonoBehaviour
{
    public BlockRegistry registry;
    public Material previewMat;

    private CraftBlueprint craftBlueprint;

    [SerializeField]
    private CraftWorldModel preview;

    public int selectedIndex = 0;
    public bool debugPlacement = false; // enable to log hit/normal/cell calculations

    BlockDef selected => registry.GetByIndex(selectedIndex);

    void OnEnable()
    {
        craftBlueprint = new CraftBlueprint();
        preview.SetBlueprint(craftBlueprint, true);
    }

    // Final placement
    void TryPlaceBlock(Vector3Int position, BlockDef blockDef, Quaternion rotation)
    {
        if (craftBlueprint == null || blockDef == null || position == null) return;

        var rotatedScale = rotation * blockDef.size;

        var newPosition = position + Vector3Int.RoundToInt((rotatedScale - Vector3.one) * 0.5f); // center the block in the grid cell

        // Check if the block already exists at this position
        if (craftBlueprint.blocks.ContainsKey(newPosition))
        {
            Debug.LogWarning($"Block already exists at {position}. Cannot place again.");
            return;
        }
        Debug.Log($"Placing block {blockDef.displayName} at {position} with rotation {rotation}");
        // CraftBlueprint currently doesn't store rotation; pass overwrite=false
        craftBlueprint.AddBlock(position, blockDef, rotation, overwrite: false);
    }

    public void TryRemoveBlock(Vector3Int position)
    {
        if (craftBlueprint == null || position == null) return;

        // Check if the block exists at this position
        if (!craftBlueprint.blocks.ContainsKey(position))
        {
            Debug.LogWarning($"No block found at {position}. Cannot remove.");
            return;
        }

        craftBlueprint.RemoveBlock(position);
    }

    public void TryPlaceBlockNextToSelected(BlockDef block, Vector3Int position, Vector3 normal, Quaternion rotation)
    {
        if (craftBlueprint == null || selected == null) return;
        Debug.Log($"Trying to place block {block.displayName} next to selected at {position} with normal {normal} and rotation {rotation}");
        // Calculate the next position based on the current selection
        Vector3Int nextPosition = position + Vector3Int.RoundToInt(normal.normalized); // convert direction to grid offset

        // Place the block at the calculated position
        TryPlaceBlock(nextPosition, block, rotation);
    }

}
