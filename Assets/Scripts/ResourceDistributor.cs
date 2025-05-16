using UnityEngine;
using Catan.UI;              // For LeftMenuUI and PlayerInventory
using Catan.TerrainGeneration;

public class ResourceDistributor : MonoBehaviour
{
    [SerializeField] private ConnectionSpawner connectionSpawner;
    [SerializeField] private LeftMenuUI leftMenuUI;

    public void AddResourcesForRoll(int rolledNumber)
    {
        if (rolledNumber == 7) return; // handle robber elsewhere

        if (connectionSpawner == null || leftMenuUI == null)
        {
            Debug.LogError("Missing references");
            return;
        }

        var inventory = leftMenuUI.localInventory;
        foreach (var cell in connectionSpawner.HexCells)
        {
            if (cell == null) continue;
            if (cell.NumberToken == rolledNumber && cell.GetResource() != ResourceType.Desert)
            {
                AddResource(inventory, cell.GetResource());
                Debug.Log($"Added 1 {cell.GetResource()} from {cell.coordinates}");
            }
        }
    }

    private void AddResource(PlayerInventory inventory, ResourceType resource)
    {
        switch (resource)
        {
            case ResourceType.Brick: inventory.Brick++; break;
            case ResourceType.Lumber: inventory.Lumber++; break;
            case ResourceType.Wool: inventory.Wool++; break;
            case ResourceType.Grain: inventory.Grain++; break;
            case ResourceType.Ore: inventory.Ore++; break;
        }
    }
}
