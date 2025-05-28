using Assets.Scripts.Enums;
using UnityEngine;

public class StructureManager : MonoBehaviour
{
    public static StructureManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Attempts to place or upgrade a structure on the target VertexPoint.
    /// </summary>
    /// <param name="vp">The target vertex.</param>
    /// <param name="type">The desired structure type.</param>
    /// <returns>True if the structure was placed or upgraded successfully.</returns>
    public bool TryPlaceStructure(VertexPoint vp, StructureType type)
    {
        if (vp == null)
        {
            Debug.LogWarning("StructureManager: No VertexPoint provided.");
            return false;
        }

        if (type == StructureType.SETTLEMENT)
        {
            if (vp.owner != null)
            {
                Debug.Log("Vertex already occupied.");
                return false;
            }

            vp.Build(StructureType.SETTLEMENT);
            return true;
        }

        if (type == StructureType.CITY)
        {
            if (vp.owner == null)
            {
                Debug.Log("Must have a settlement before upgrading to city.");
                return false;
            }

            // Only allow upgrade if structure is a Settlement
            bool isSettlement = vp.transform.Find("Settlement")?.gameObject.activeSelf == true;

            if (!isSettlement)
            {
                Debug.Log("Cannot upgrade to city: no settlement present.");
                return false;
            }

            vp.Build(StructureType.CITY);
            return true;
        }

        Debug.LogWarning("Unsupported structure type at vertex.");
        return false;
    }

    public bool TryPlaceRoad(EdgePoint ep, string player)
    {
        if (ep == null)
        {
            Debug.Log("StructureManager: Invalid EdgePoint.");
            return false;
        }

        if (ep.owner != null)
        {
            Debug.Log("StructureManager: Edge already has a road.");
            return false;
        }

        if (!ep.IsConnectedToPlayer(player))
        {
            Debug.Log("StructureManager: No connection to existing player road or structure.");
            return false;
        }

        ep.Build(player);
        return true;
    }
}
