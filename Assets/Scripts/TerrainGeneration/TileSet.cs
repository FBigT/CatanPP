// Assets/Scripts/TerrainGeneration/TileSet.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Catan/Tile Set", fileName = "TileSet")]
public class TileSet : ScriptableObject
{
    public GameObject claypit;
    public GameObject mountain;
    public GameObject pasture;
    public GameObject sand;
    public GameObject wood;
    public GameObject desert;     // yellow / no-resource
}
