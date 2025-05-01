// Assets/Scripts/TerrainGeneration/ResourceType.cs
//
// A single enum understood by *all* terrain / placement scripts.
// ────────────────────────────────────────────────────────────────

/// <summary>
/// Resource symbols used by both the Unity client and the backend.
/// New names (Brick, Ore…) are aliases of the old ones so nothing breaks.
/// </summary>
public enum ResourceType
{
    // ── original names (kept for older scripts) ───────────────
    Wood = 0,
    Stone = 1,
    Wheat = 2,
    Clay = 3,
    Sheep = 4,
    Desert = 5,

    // ── backend / HexSpawner aliases – point to the same values ─
    Lumber = Wood,
    Ore = Stone,
    Grain = Wheat,
    Brick = Clay,
    Wool = Sheep
}
