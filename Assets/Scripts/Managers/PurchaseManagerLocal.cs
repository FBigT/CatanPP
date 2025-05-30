using Assets.Scripts.Enums;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseManagerLocal : MonoBehaviour
{
    public int Brick, Lumber, Wool, Grain, Ore;

    public bool HasEnoughFor(PurchaseType type)
    {
        return type switch
        {
            PurchaseType.Road => Brick >= 1 && Lumber >= 1,
            PurchaseType.Settlement => Brick >= 1 && Lumber >= 1 && Wool >= 1 && Grain >= 1,
            PurchaseType.City => Grain >= 2 && Ore >= 3,
            _ => false
        };
    }

    public void SpendResources(PurchaseType type)
    {
        switch (type)
        {
            case PurchaseType.Road:
                Brick--; Lumber--;
                break;
            case PurchaseType.Settlement:
                Brick--; Lumber--; Wool--; Grain--;
                break;
            case PurchaseType.City:
                Grain -= 2; Ore -= 3;
                break;
        }
    }
}

[System.Serializable]
public struct PurchaseEntry
{
    public PurchaseType type;
    public Button button;
    public GameObject prefab;
    public KeyCode key;
}