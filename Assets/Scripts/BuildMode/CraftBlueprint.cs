// ============================================================================
// CraftBlueprint.cs — data & logic for a craft blueprint (in-memory / file)
// ============================================================================
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CraftBlueprint: a pure data container + logic for a craft layout on a discrete grid.
/// Stores mapping from grid coordinate to BlockDef, plus metadata (name + Guid id).
/// Can be serialized to / deserialized from JSON (BlockDefs resolved through a registry).
/// </summary>
public class CraftBlueprint
{
	// Grid: anchor cell -> BlockDef
	public readonly Dictionary<Vector3Int, BlueprintBlock> blocks = new();

	public string craftName;
	public Guid id;

	// --------------------------- Observability ------------------------------
	public event Action<Vector3Int, BlockDef> BlockPlacedOrUpdated; // fired after add/overwrite
	public event Action<Vector3Int> BlockRemoved; // fired after removal
	public event Action BlueprintCleared; // fired after Clear()
	public CraftBlueprint(string name = "Unnamed")
	{
		craftName = name;
		id = Guid.NewGuid();
	}

	// ----------------------------- Public API -----------------------------
	public bool AddBlock(Vector3Int cell, BlockDef def, Quaternion rotation, bool overwrite = false)
	{
		if (def == null) return false;
		if (blocks.ContainsKey(cell) && !overwrite) return false;
		blocks[cell] = new BlueprintBlock
		{
			def = def,
			gridPos = cell,
			rotation = rotation
		};
		BlockPlacedOrUpdated?.Invoke(cell, def);
		return true;
	}

	public bool RemoveBlock(Vector3Int cell)
	{
		if (!blocks.ContainsKey(cell)) return false;
		blocks.Remove(cell);
		BlockRemoved?.Invoke(cell);
		return true;
	}

	/// <summary>Remove all blocks and notify observers once.</summary>
	public void Clear()
	{
		if (blocks.Count == 0) return;
		blocks.Clear();
		BlueprintCleared?.Invoke();
	}

	/// <summary>
	/// Checks that all placed blocks form a single 6-neighbour connected component.
	/// Empty blueprint counts as connected.
	/// </summary>
	public bool CheckConnectivity()
	{
		if (blocks.Count <= 1) return true;
		using var enumerator = blocks.Keys.GetEnumerator();
		enumerator.MoveNext();
		var start = enumerator.Current;
		var visited = new HashSet<Vector3Int>();
		var q = new Queue<Vector3Int>();
		q.Enqueue(start);
		visited.Add(start);
		static IEnumerable<Vector3Int> Neigh(Vector3Int p)
		{
			yield return p + Vector3Int.right; yield return p + Vector3Int.left;
			yield return p + Vector3Int.up; yield return p + Vector3Int.down;
			yield return p + new Vector3Int(0,0,1); yield return p + new Vector3Int(0,0,-1);
		}
		while (q.Count > 0)
		{
			var cur = q.Dequeue();
			foreach (var n in Neigh(cur))
			{
				if (!blocks.ContainsKey(n) || visited.Contains(n)) continue;
				visited.Add(n); q.Enqueue(n);
			}
		}
		return visited.Count == blocks.Count;
	}

	// --------------------------- Serialization ----------------------------
	[Serializable]
	private class SerializableData
	{
		public string craftName;
		public string id;
		public List<Entry> entries = new();
	}

	[Serializable]
	private class Entry
	{
		public Vector3Int pos;
		public string defId;
		public Quaternion rotation;
	}

	/// <summary>
	/// Serialize to JSON (stores only def IDs and positions).
	/// </summary>
	public string Serialize()
	{
		var data = new SerializableData
		{
			craftName = craftName,
			id = id.ToString()
		};
		foreach (var kv in blocks)
		{
			data.entries.Add(new Entry { pos = kv.Key, defId = kv.Value.def.id, rotation = kv.Value.rotation });
		}
		return JsonUtility.ToJson(data, true);
	}

	/// <summary>
	/// Deserialize from JSON, resolving BlockDefs by id via registry.
	/// Unknown defs are skipped.
	/// </summary>
	public static CraftBlueprint Deserialize(string json, BlockRegistry registry)
	{
		if (string.IsNullOrWhiteSpace(json)) return new CraftBlueprint();
		var data = JsonUtility.FromJson<SerializableData>(json);
		var bp = new CraftBlueprint(data?.craftName ?? "Unnamed");
		if (Guid.TryParse(data?.id, out var parsed)) bp.id = parsed; else bp.id = Guid.NewGuid();
		if (data?.entries != null && registry != null)
		{
			registry.Init();
			foreach (var e in data.entries)
			{
				var def = registry.GetById(e.defId);
				if (def != null) bp.blocks[e.pos] = new BlueprintBlock
				{
					def = def,
					gridPos = e.pos,
					rotation = e.rotation
				};
			}
		}
		return bp;
	}
}
