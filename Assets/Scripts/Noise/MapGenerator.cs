// Assets/Scripts/Noise/MapGenerator.cs
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MapDisplay))]
public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColorMap, Mesh }

    [Header("General")]
    [SerializeField] DrawMode drawMode = DrawMode.Mesh;
    [SerializeField, Range(0, 6)] int levelOfDetail = 1;
    [SerializeField] bool autoUpdate = false;

    [Header("Noise")]
    const int mapChunkSize = 241;                      // fixed – keep in sync with MeshGenerator
    [SerializeField, Min(1)] float noiseScale = 50f;
    [SerializeField, Min(1)] int octaves = 4;
    [SerializeField, Range(0, 1)] float persistance = .4f;
    [SerializeField, Min(1)] float lacunarity = 2f;
    [SerializeField] int seed = 0;
    [SerializeField] Vector2 offset;

    [Header("Mesh")]
    [SerializeField] float meshHeightMultiplier = 10f;
    [SerializeField] AnimationCurve meshHeightCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Colours")]
    [SerializeField] TerrainType[] terrainTypes = new TerrainType[0];

    MapDisplay _display;

    /* ------------------------------------------------ lifecycle */

    void Awake() => _display = GetComponent<MapDisplay>();
    void Start() => TryGenerate("Start");              // generate once when the scene loads
    void OnValidate() { if (_display == null) _display = GetComponent<MapDisplay>(); }

    /* ------------------------------------------------ public */

    /// <summary>Used by the custom Inspector button and by CampaignGameMode.</summary>
    public void GenerateMap() => TryGenerate("Inspector/Script");

    public bool AutoUpdate => autoUpdate;

    /* ------------------------------------------------ internals */

    void TryGenerate(string source)
    {
        if (!ValidateParameters()) return;
        Debug.Log($"[MapGenerator] Generating map ({source}), seed={seed}");

        float[,] noiseMap = Noise.GenerateNoiseMap(
            mapChunkSize, mapChunkSize,
            seed, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colours = BuildColourMap(noiseMap);

        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                _display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
                break;

            case DrawMode.ColorMap:
                _display.DrawTexture(TextureGenerator.TextureFromColorMap(colours, mapChunkSize, mapChunkSize));
                break;

            case DrawMode.Mesh:
                var tex = TextureGenerator.TextureFromColorMap(colours, mapChunkSize, mapChunkSize);
                var mesh = MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail);
                _display.DrawMesh(mesh, tex);
                break;

        }
        Debug.Log($"[MapGenerator] sample(0,0) = {noiseMap[0, 0]:F3}");
    }

    bool ValidateParameters()
    {
        if (terrainTypes.Length == 0)
        {
            Debug.LogError("[MapGenerator] No terrain types defined - nothing to draw.");
            return false;
        }
        if (noiseScale <= 0)
            Debug.LogWarning("[MapGenerator] noiseScale ≤ 0 – terrain will be uniform.");

        return true;
    }

    Color[] BuildColourMap(float[,] noiseMap)
    {
        Color[] colours = new Color[mapChunkSize * mapChunkSize];

        for (int y = 0; y < mapChunkSize; y++)
            for (int x = 0; x < mapChunkSize; x++)
            {
                float h = noiseMap[x, y];
                for (int i = 0; i < terrainTypes.Length; i++)
                    if (h <= terrainTypes[i].height)
                    { colours[y * mapChunkSize + x] = terrainTypes[i].color; break; }
            }
        return colours;
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    [Range(0, 1)] public float height;
    public Color color;
}

/* ---------- custom Inspector (unchanged except for button copy) ---------- */
#if UNITY_EDITOR
[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var gen = (MapGenerator)target;

        if (DrawDefaultInspector() && gen.AutoUpdate) gen.GenerateMap();
        if (GUILayout.Button("Generate")) gen.GenerateMap();
    }
}
#endif
