// ============================================================================
// Scripts/BuildMode/InventoryUI.cs — simple block selection inventory (UI agnostic)
// ============================================================================
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public PlacementController cursor;
    public BlockRegistry registry;

    void OnGUI()
    {
        if (registry == null || registry.blocks == null) return;

        // Place at the bottom center of the screen
        float width = 520;
        float height = 80;
        float x = (Screen.width - width) / 2;
        float y = Screen.height - height - 10;
        GUILayout.BeginArea(new Rect(x, y, width, height), GUI.skin.box);

        GUILayout.BeginHorizontal();
        var prevBg = GUI.backgroundColor;
        for (int i = 0; i < registry.blocks.Count && i < 9; i++)
        {
            var b = registry.blocks[i];
            bool isSelected = (cursor != null && cursor.selectedIndex == i);

            // highlight selected slot
            GUI.backgroundColor = isSelected ? new Color(0.2f, 0.6f, 1f, 1f) : prevBg;

            if (GUILayout.Button($"{i + 1}\n{b.displayName}\n[{b.category}]", GUILayout.Width(56), GUILayout.Height(56)))
            {
                if (cursor != null)
                {
                    //TODO Fix it
                    // call public API directly (safer than SendMessage)
                    //cursor.SetSelectedIndex(i);
                }
                else
                {
                    Debug.LogWarning("InventoryUI: cursor reference is not assigned in the Inspector.");
                }
            }
        }
        GUI.backgroundColor = prevBg;
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }
}
