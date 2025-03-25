using UnityEngine;
using System.Collections.Generic;

public class HexTileGenerator : MonoBehaviour
{
    public SO_HexTile biome; // The biome assigned to this tile
    public MeshRenderer tileRenderer; // Renderer for the tile
    public MeshFilter tileMeshFilter; // Mesh filter for vertex manipulation
    public Transform environmentParent; // Parent object for spawned environmental objects

    [Header("Hex Tile Settings")]
    public float hexSize = 1f; // Size of the hex tile
    public bool isNearWater = false; // Set this if the tile is near water

    private Mesh hexMesh;

    void Start()
    {
        GenerateHexTile();
    }

    void GenerateHexTile()
    {
        if (biome == null)
        {
            Debug.LogWarning("No biome assigned to hex tile!");
            return;
        }

        // Generate the hex mesh with displacement
        GenerateHexMesh();

        // Apply biome texture
        ApplyBiomeTexture();

        // Spawn environmental objects
        SpawnEnvironmentalObjects();
    }

    void GenerateHexMesh()
    {
        hexMesh = new Mesh();
        tileMeshFilter.mesh = hexMesh;

        Vector3[] vertices = new Vector3[7]; // 6 corners + center
        int[] triangles = new int[18]; // 6 triangles (3 indices each)

        float heightOffset = biome.heightVariation * Mathf.PerlinNoise(transform.position.x * biome.noiseScale, transform.position.z * biome.noiseScale);

        if (biome.allowLargeDisplacement)
        {
            heightOffset *= biome.displacementStrength;
        }

        // Generate Hexagonal Vertices
        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.PI / 3 * i;
            float x = Mathf.Cos(angle) * hexSize;
            float z = Mathf.Sin(angle) * hexSize;
            float y = Mathf.Lerp(biome.minHeight, biome.maxHeight, heightOffset);

            // Smooth transition near water
            if (isNearWater && biome.hasWaterEdgeTransition)
            {
                y = Mathf.Lerp(y, 0, 0.5f);
            }

            vertices[i] = new Vector3(x, y, z);
        }
        vertices[6] = new Vector3(0, Mathf.Lerp(biome.minHeight, biome.maxHeight, heightOffset), 0); // Center vertex

        // Create Triangles (hexagonal shape)
        for (int i = 0; i < 6; i++)
        {
            int next = (i + 1) % 6;
            int triIndex = i * 3;

            triangles[triIndex] = 6; // Center
            triangles[triIndex + 1] = i;
            triangles[triIndex + 2] = next;
        }

        hexMesh.vertices = vertices;
        hexMesh.triangles = triangles;
        hexMesh.RecalculateNormals();
    }

    void ApplyBiomeTexture()
    {
        if (tileRenderer != null && biome.biomeTexture != null)
        {
            tileRenderer.material.mainTexture = biome.biomeTexture;
            tileRenderer.material.color = biome.biomeColor;
            tileRenderer.material.mainTextureScale = new Vector2(biome.textureScale, biome.textureScale);
        }
    }

    void SpawnEnvironmentalObjects()
    {
        if (biome.environmentalLayers == null || biome.environmentalLayers.Length == 0)
            return;

        foreach (var layer in biome.environmentalLayers)
        {
            if (layer.objects.Length == 0)
                continue;

            float spawnChance = Random.Range(0f, 1f);
            if (spawnChance > layer.spawnProbability)
                continue; // Skip this object if it fails probability check

            GameObject objToSpawn = layer.objects[Random.Range(0, layer.objects.Length)];
            if (objToSpawn == null)
                continue;

            // Pick a random point on the hex tile
            Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-hexSize / 2, hexSize / 2), 0, Random.Range(-hexSize / 2, hexSize / 2));

            float heightNoise = Mathf.PerlinNoise(spawnPosition.x * biome.noiseScale, spawnPosition.z * biome.noiseScale);
            float objectHeight = Mathf.Lerp(biome.minHeight, biome.maxHeight, heightNoise);

            // Apply height constraints
            if (objectHeight < layer.minHeight || objectHeight > layer.maxHeight)
                continue;

            spawnPosition.y = objectHeight;

            GameObject newObject = Instantiate(objToSpawn, spawnPosition, Quaternion.identity, environmentParent);
        }
    }
}
