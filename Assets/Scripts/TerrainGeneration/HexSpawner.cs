// Assets/Scripts/TerrainGeneration/HexSpawner.cs
using UnityEngine;

namespace Catan.TerrainGeneration
{
    /// <summary>
    /// Replaces the temporary HexCell planes produced by MapGenerator
    /// with your coloured tile prefabs from <see cref="TileSet"/>.
    /// </summary>
    [RequireComponent(typeof(MapGenerator))]
    public class HexSpawner : MonoBehaviour
    {
        [SerializeField] TileSet tiles;
        [SerializeField] Transform holder;   // optional empty parent

        void Awake()
        {
            var gen = GetComponent<MapGenerator>();
            gen.GenerateMap();          // creates logical HexCell objects
            SpawnPrefabs();
        }

        /* ----------------------------------------------------------- */
        void SpawnPrefabs()
        {
            foreach (HexCell cell in FindObjectsByType<HexCell>(FindObjectsSortMode.None))
            {
                GameObject prefab = PickPrefab(cell.GetResource());

                Instantiate(prefab, cell.transform.position,
                            Quaternion.identity, holder);

                Destroy(cell.gameObject);          // remove proto-plane
            }
        }

        /* ----------------------------------------------------------- */
        GameObject PickPrefab(ResourceType res) => res switch
        {
            ResourceType.Brick => tiles.claypit,
            ResourceType.Ore => tiles.mountain,
            ResourceType.Wool => tiles.pasture,
            ResourceType.Grain => tiles.sand,
            ResourceType.Lumber => tiles.wood,
            _ => tiles.desert        // Desert / fallback
        };
    }
}
