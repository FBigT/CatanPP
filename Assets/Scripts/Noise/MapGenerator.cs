using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] public DrawMode drawMode;
    [SerializeField] const int mapChunkSize = 241;
    [SerializeField, Range(0, 6)] private int levelOfDetail;
    [SerializeField] private float noiseScale;
    [SerializeField, Min(0)] private int octaves = 0;
    [SerializeField, Min(1)] private float lacunarity = 1;
    [SerializeField, Range(0, 1)] private float persistance;
    [SerializeField] private int seed;
    [SerializeField] private Vector2 offset;
    [SerializeField] private float MeshHeightMultiplier;
    [SerializeField] private AnimationCurve MeshHeightCurve;
    [SerializeField] private bool autoUpdate;
    [SerializeField] private TerrainType[] terrainTypes;

    private MapDisplay mapDisplay;

    public enum DrawMode { NoiseMap, ColorMap, Mesh }
    public bool AutoUpdate { get => autoUpdate; }

    private void Awake()
    {
        mapDisplay = GetComponent<MapDisplay>();
    }

    private void OnValidate()
    {
        if (mapDisplay == null)
            mapDisplay = GetComponent<MapDisplay>();
    }

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colors = new Color[mapChunkSize * mapChunkSize];

        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < terrainTypes.Length; i++)
                {
                    if (currentHeight <= terrainTypes[i].height)
                    {
                        colors[y * mapChunkSize + x] = terrainTypes[i].color;
                        break;
                    }
                }
            }
        }

        if (drawMode == DrawMode.NoiseMap)
        {
            Texture2D texture = TextureGenerator.TextureFromHeightMap(noiseMap);
            mapDisplay.DrawTexture(texture);
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            Texture2D texture = TextureGenerator.TextureFromColorMap(colors, mapChunkSize, mapChunkSize);
            mapDisplay.DrawTexture(texture);
        }
        else if (drawMode == DrawMode.Mesh)
        {
            Texture2D texture = TextureGenerator.TextureFromColorMap(colors, mapChunkSize, mapChunkSize);
            mapDisplay.DrawMesh(
                MeshGenerator.GenerateTerrainMesh(noiseMap, MeshHeightMultiplier, MeshHeightCurve, levelOfDetail),
                texture
                );
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

#if UNITY_EDITOR
[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    private MapGenerator gen;

    private void OnEnable()
    {
        gen = (MapGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        if (DrawDefaultInspector())
        {
            if (gen.AutoUpdate)
                gen.GenerateMap();
        }

        if (GUILayout.Button("Generate"))
        {
            gen.GenerateMap();
        }
    }
}
#endif
