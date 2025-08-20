// ============================================================================
// CraftPreview.cs — runtime visual representation of a CraftBlueprint
// ============================================================================
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CraftPreview maintains instantiated GameObjects that mirror the logical data in a CraftBlueprint.
/// </summary>
public class CraftWorldModel : MonoBehaviour
{
    public BlockRegistry registry; // injected through editor or script
	[Tooltip("Source blueprint data to visualize")] private CraftBlueprint blueprint;
	[Tooltip("Optional parent transform for spawned blocks")] public Transform contentRoot;

	// grid cell -> instance GameObject
	public readonly Dictionary<Vector3Int, GameObject> instances = new();

	void Awake()
	{
		if (!contentRoot) contentRoot = this.transform;
	}

    void OnEnable()
    {
        if (blueprint == null) return;
        Subscribe();
        // Optional initial build
        BuildFromBlueprint();
    }

    public void SetBlueprint(CraftBlueprint newBlueprint, bool rebuild = true)
    {
        if (blueprint == newBlueprint) return;
        Unsubscribe();
        blueprint = newBlueprint;
        Subscribe();
        if (rebuild) BuildFromBlueprint();
    }

	void OnDisable()
	{
        if (blueprint != null)
		Unsubscribe();
	}

	void Subscribe()
	{
		if (blueprint == null) return;
		blueprint.BlockPlacedOrUpdated += OnBlockChanged;
		blueprint.BlockRemoved += OnBlockRemoved;
		blueprint.BlueprintCleared += OnBlueprintCleared;
	}

	void Unsubscribe()
	{
		if (blueprint == null) return;
		blueprint.BlockPlacedOrUpdated -= OnBlockChanged;
		blueprint.BlockRemoved -= OnBlockRemoved;
		blueprint.BlueprintCleared -= OnBlueprintCleared;
	}

	/// <summary>Create all instances from the current blueprint (clears previous).</summary>
	public void BuildFromBlueprint()
	{
		if (blueprint == null) return;
		ClearAll();
		foreach (var kv in blueprint.blocks)
		{
			SpawnBlock(kv.Key, kv.Value);
		}
	}

	/// <summary>Update or create a single block instance to reflect blueprint changes.</summary>
	public void UpdateBlock(Vector3Int cell)
	{
		if (blueprint == null) return;
		// Determine if block should exist
		if (!blueprint.blocks.TryGetValue(cell, out var block))
		{
			// Should be removed
			if (instances.TryGetValue(cell, out var go) && go)
			{
				Destroy(go);
				instances.Remove(cell);
			}
			return;
		}
		// Block should exist
		if (instances.TryGetValue(cell, out var existing) && existing)
		{
			// Replace if prefab mismatch
			var marker = existing.GetComponent<BlockDef>();
			if (marker == block.def) return; // already correct
			Destroy(existing);
			instances.Remove(cell);
		}
		SpawnBlock(cell, block);
	}

	/// <summary>Remove all instantiated blocks.</summary>
	public void ClearAll()
	{
		foreach (var kv in instances)
		{
			if (kv.Value) Destroy(kv.Value);
		}
		instances.Clear();
	}

	// ----------------------- Event Handlers -----------------------
	void OnBlockChanged(Vector3Int cell, BlockDef def) => UpdateBlock(cell);
	void OnBlockRemoved(Vector3Int cell) => UpdateBlock(cell); // same logic removes
	void OnBlueprintCleared() => ClearAll();

	void SpawnBlock(Vector3Int cell, BlueprintBlock block)
	{
		if (!block.def || !block.def.prefab) return;
		var go = Instantiate(block.def.prefab, contentRoot);
		go.transform.position = GridUtil.GridToWorldCenter(cell); // assumes 1x1x1 and center-based grid
		go.transform.rotation = block.rotation;
		go.transform.localScale = Vector3.Scale(Vector3.one * GridUtil.CellSize, (Vector3)block.def.size);
		var marker = go.AddComponent<CraftPreviewMarker>();
		marker.def = block.def;
		marker.gridPos = cell;
		instances[cell] = go;
		Debug.Log($"Spawned block {block.def.displayName} at {cell} -> {GridUtil.GridToWorldCenter(cell)} with rotation {block.rotation.eulerAngles}");
	}
}

/// <summary>Helper component to tag preview instances with their BlockDef.</summary>
public class CraftPreviewMarker : MonoBehaviour
{
    public Vector3Int gridPos;
	public BlockDef def;
}

