using System.Collections.Generic;
using UnityEngine;

// Collects build-mode log messages (e.g., save/load actions) for optional UI display.
public class BuildLogger : MonoBehaviour
{
    [Range(10, 1000)] public int maxEntries = 200;
    public readonly List<string> entries = new List<string>();

    public void Log(string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        if (entries.Count >= maxEntries) entries.RemoveAt(0);
        entries.Add(message);
        Debug.Log("[Build] " + message);
    }
}
