using UnityEngine;

[CreateAssetMenu(fileName = "NewBiome", menuName = "Terrain/Biome")]
public class SO_HexTile : ScriptableObject
{
    [Header("Biome Settings")]
    public string biomeName;
    public Texture2D biomeTexture;
    public Color biomeColor;

    [Header("Height Settings")]
    public float minHeight = 0f;
    public float maxHeight = 5f;
    public float heightVariation = 1f;

    [Header("Blending Settings")]
    public float blendStrength = 0.5f;
    public float textureScale = 1f;

    [Header("Noise Settings")]
    public float noiseScale = 1f;
    public float noiseOffset = 0f;

    [Header("Displacement Settings")]
    public float displacementStrength = 0.2f;
    public bool allowLargeDisplacement = false;

    [Header("Environmental Objects")]
    public EnvironmentalObjectLayer[] environmentalLayers;

    [Header("Edge Transitions")]
    public bool hasWaterEdgeTransition = false;
    public EdgeTransitionType waterEdgeType = EdgeTransitionType.None;
}

[System.Serializable]
public class EnvironmentalObjectLayer
{
    public string layerName;
    public GameObject[] objects;
    public float spawnProbability;
    public float minHeight;
    public float maxHeight;
}

public enum EdgeTransitionType
{
    None,
    SandyBeach,
    RockyCliff
}
