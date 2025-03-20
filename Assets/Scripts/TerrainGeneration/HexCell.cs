using UnityEngine;

[CreateAssetMenu(fileName = "NewHexCell", menuName = "HexGrid/HexCell")]
public class HexCell : ScriptableObject
{
    public enum CellType
    {
        Grass,
        Water,
        Mountain,
        Desert,
        Forest
    }

    [SerializeField] string cellName;
    [SerializeField] CellType hexCellType;
    [SerializeField] float elevationMin;
    [SerializeField] float elevationMax;


    public string CellName { get { return cellName; } }
    public CellType HexCellType { get { return hexCellType; } }
    public float ElevationMin { get { return elevationMin; } }
    public float ElevationMax { get { return elevationMax; } }
}
