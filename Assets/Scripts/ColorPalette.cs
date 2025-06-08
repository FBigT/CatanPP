using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorPalette", menuName = "Game/Color Palette")]
public class ColorPalette : ScriptableObject
{
    [SerializeField] private List<Color> availableColors = new();
    private readonly HashSet<Color> usedColors = new();

    /// <summary>
    /// Get a free color and mark it as used.
    /// </summary>
    public bool TryGetColor(out Color color)
    {
        foreach (var col in availableColors)
        {
            if (!usedColors.Contains(col))
            {
                usedColors.Add(col);
                color = col;
                return true;
            }
        }

        color = Color.clear;
        return false;
    }

    /// <summary>
    /// Releases a previously used color so it can be reassigned.
    /// </summary>
    public void ReleaseColor(Color color)
    {
        usedColors.Remove(color);
    }

    /// <summary>
    /// Resets all color assignments.
    /// </summary>
    public void ResetPalette()
    {
        usedColors.Clear();
    }

    /// <summary>
    /// Check which colors are still free.
    /// </summary>
    public List<Color> GetAvailableColors()
    {
        return availableColors.FindAll(c => !usedColors.Contains(c));
    }

    /// <summary>
    /// Check which colors are in use.
    /// </summary>
    public List<Color> GetUsedColors()
    {
        return new List<Color>(usedColors);
    }
}
