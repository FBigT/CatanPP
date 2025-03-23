using UnityEngine;
[System.Serializable]
public class CityResponse
{
    public long id;
    public string owner;
    public int x;
    public int y;
    public bool upgraded; // ✅ Matches API response exactly
}
