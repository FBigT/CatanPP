using UnityEngine;
using System;

public class YearOfPlentyPanel : MonoBehaviour
{
    public event Action<string, string> OnResourcesSelected;

    public void Show()
    {
        Debug.Log("Year of Plenty panel shown - implement resource selection UI");

        // For testing, auto-select default resources after 1 second
        Invoke(nameof(SelectDefaultResources), 1f);
    }

    private void SelectDefaultResources()
    {
        OnResourcesSelected?.Invoke("wood", "brick");
    }
}
