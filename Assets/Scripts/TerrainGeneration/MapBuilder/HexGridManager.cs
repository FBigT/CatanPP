using System.Collections.Generic;
using UnityEngine;

public class HexGridManager : MonoBehaviour
{
    public float radius = 1f;
    public GameObject tilePrefab;
    public Camera mainCamera;

    private Dictionary<Vector2Int, GameObject> placedTiles = new();
    private TileMapPopulator tileSelector;

    private void Start()
    {
        tileSelector = FindFirstObjectByType<TileMapPopulator>();
        if (!mainCamera) mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                Vector3 worldPos = hit.point;
                Vector2Int hexCoords = WorldToHexCoords(worldPos);

                PlaceTile(hexCoords);
            }
        }
    }

    void PlaceTile(Vector2Int coords)
    {
        if (tileSelector == null || tileSelector.GetSelectedTile() == null)
            return;

        if (placedTiles.TryGetValue(coords, out GameObject existing))
        {
            Destroy(existing);
        }

        Vector3 pos = HexToWorldPosition(coords);
        var tileGO = Instantiate(tileSelector.GetSelectedTile().tilePrefab, pos, Quaternion.identity);
        placedTiles[coords] = tileGO;
    }

    Vector3 HexToWorldPosition(Vector2Int hex)
    {
        float x = radius * 3f / 2f * hex.x;
        float z = radius * Mathf.Sqrt(3) * (hex.y + hex.x / 2f);
        return new Vector3(x, 0f, z);
    }

    Vector2Int WorldToHexCoords(Vector3 worldPos)
    {
        float q = (2f / 3f * worldPos.x) / radius;
        float r = (-1f / 3f * worldPos.x + Mathf.Sqrt(3) / 3f * worldPos.z) / radius;

        return HexRound(q, r);
    }

    Vector2Int HexRound(float q, float r)
    {
        float x = q;
        float z = r;
        float y = -x - z;

        int rx = Mathf.RoundToInt(x);
        int ry = Mathf.RoundToInt(y);
        int rz = Mathf.RoundToInt(z);

        float dx = Mathf.Abs(rx - x);
        float dy = Mathf.Abs(ry - y);
        float dz = Mathf.Abs(rz - z);

        if (dx > dy && dx > dz) rx = -ry - rz;
        else if (dy > dz) ry = -rx - rz;
        else rz = -rx - ry;

        return new Vector2Int(rx, rz);
    }
}
