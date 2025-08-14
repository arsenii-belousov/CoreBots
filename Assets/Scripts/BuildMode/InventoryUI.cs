// ============================================================================
// Scripts/BuildMode/InventoryUI.cs — simple block selection inventory (UI agnostic)
// ============================================================================
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public BuildCursor cursor;
    public BlockRegistry registry;

    void OnGUI()
    {
        if (registry == null || registry.blocks == null) return;
        GUILayout.BeginArea(new Rect(10, 10, 520, 80), GUI.skin.box);
        GUILayout.Label($"Inventory (1-9) | Snap: {(cursor && cursor.snapToGrid ? "ON" : "OFF")} | Total HP: {cursor.assembler.totalHP} | Net Energy: {cursor.assembler.GetNetEnergy()}");
        GUILayout.BeginHorizontal();
        for (int i = 0; i < registry.blocks.Count && i < 9; i++)
        {
            var b = registry.blocks[i];
            GUI.enabled = true;
            if (GUILayout.Button($"{i + 1}. {b.displayName}\n[{b.category}]"))
            {
                cursor.selectedIndex = i;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}
