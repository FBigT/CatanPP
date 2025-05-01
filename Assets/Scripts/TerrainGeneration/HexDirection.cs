// Assets/Scripts/TerrainGeneration/HexDirection.cs
// -------------------------------------------------
// Central definition of axial directions for every
// hex-grid script (cells, grids, path-finding, etc.)

/// <summary>
/// Clock-wise axial directions starting at “north-east”.  
/// Matches the 6 neighbours array used by <c>HexCell.Neighbors</c>.
/// </summary>
public enum HexDirection
{
    NE = 0,
    E = 1,
    SE = 2,
    SW = 3,
    W = 4,
    NW = 5
}

/// <summary>Utility functions for <see cref="HexDirection"/>.</summary>
public static class HexDirectionExtensions
{
    /// <returns>The opposite (180°) direction.</returns>
    public static HexDirection Opposite(this HexDirection dir) =>
        (int)dir < 3 ? dir + 3 : dir - 3;
}
