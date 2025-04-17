using UnityEngine;

[CreateAssetMenu(menuName = "Hex/TileDefinition")]
public class SO_Tile : ScriptableObject
{
    public string tileName;
    public Sprite icon;
    public GameObject tilePrefab;
    public int tileID;
    public bool isWalkable;
    public bool isInteractable;
}