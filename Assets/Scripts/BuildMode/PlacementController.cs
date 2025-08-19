// ============================================================================
// Scripts/BuildMode/BuildCursor.cs — aiming/preview/placement/removal
// ============================================================================
using UnityEngine;

public class PlacementController : MonoBehaviour
{
    public BlockRegistry registry;
    public Material previewMat;

    private CraftBlueprint craftBlueprint;

    public CraftWorldModel preview;

    public bool snapToGrid = true;
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
        if (craftBlueprint == null || blockDef == null) return;

        // Check if the block already exists at this position
        if (craftBlueprint.blocks.ContainsKey(position))
        {
            Debug.LogWarning($"Block already exists at {position}. Cannot place again.");
            return;
        }

        // CraftBlueprint currently doesn't store rotation; pass overwrite=false
        craftBlueprint.AddBlock(position, blockDef, rotation, overwrite: false);
    }

    void TryRemoveBlock(Vector3Int position)
    {
        if (craftBlueprint == null) return;

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

        // Calculate the next position based on the current selection
        Vector3Int nextPosition = position + Vector3Int.RoundToInt(normal); // convert direction to grid offset

        // Place the block at the calculated position
    TryPlaceBlock(nextPosition, block, rotation);
    }

}
