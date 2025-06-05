using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerColorEntry
{
    public string name;
    public Color color;
    public bool isInUse = false;
}

[CreateAssetMenu(fileName = "PlayerColorPalette", menuName = "Game/Player Color Palette")]
public class PlayerColorPalette : ScriptableObject
{
    [Header("Assignable Colors")]
    [SerializeField] private List<PlayerColorEntry> colorEntries = new();

    [Header("Debug Color (Fallback)")]
    [SerializeField] private Color debugColor = Color.magenta;

    /// <summary>
    /// Attempts to assign an unused color.
    /// </summary>
    /// <returns>The assigned color or debug color if none are available.</returns>
    public Color AssignColor(out string colorName)
    {
        foreach (var entry in colorEntries)
        {
            if (!entry.isInUse)
            {
                entry.isInUse = true;
                colorName = entry.name;
                return entry.color;
            }
        }

        // No color available, return debug fallback
        colorName = "Debug";
        return debugColor;
    }

    /// <summary>
    /// Releases a color back to the pool by name.
    /// </summary>
    public void ReleaseColor(string colorName)
    {
        foreach (var entry in colorEntries)
        {
            if (entry.name == colorName)
            {
                entry.isInUse = false;
                return;
            }
        }
    }

    /// <summary>
    /// Resets all colors to be available.
    /// </summary>
    public void ResetAll()
    {
        foreach (var entry in colorEntries)
        {
            entry.isInUse = false;
        }
    }

    /// <summary>
    /// Returns the debug color.
    /// </summary>
    public Color GetDebugColor() => debugColor;
}
