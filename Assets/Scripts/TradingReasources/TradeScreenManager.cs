using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TradeScreenManager : MonoBehaviour
{
    public GameObject mainTradePanel;
    public GameObject requestPanel;
    public GameObject offerPanel;

    public List<ResourceButtonHandler> requestResourceButtons;
    public List<ResourceButtonHandler> offerResourceButtons;

    public List<string> selectedRequestedResources = new List<string>();
    public List<int> selectedRequestedQuantities = new List<int>();

    public List<string> selectedOfferedResources = new List<string>();
    public List<int> selectedOfferedQuantities = new List<int>();

    public void OnCancelClicked(GameObject panel)
    {
        panel.SetActive(false);
        mainTradePanel.SetActive(true);
    }

    public void OnApplyClicked(GameObject panel, List<ResourceButtonHandler> resourceButtons, List<string> selectedResources, List<int> selectedQuantities)
    {
        selectedResources.Clear();
        selectedQuantities.Clear();

        foreach (var button in resourceButtons)
        {
            if (button.GetQuantity() > 0)
            {
                selectedResources.Add(button.resourceName);
                selectedQuantities.Add(button.GetQuantity());
            }
        }

        panel.SetActive(false);
        mainTradePanel.SetActive(true);


        for (int i = 0; i < selectedResources.Count; i++)
        {
            Debug.Log($"Selected Resource: {selectedResources[i]}, Quantity: {selectedQuantities[i]}");
        }
    }

    public void OpenRequestPanel()
    {
        mainTradePanel.SetActive(false);
        offerPanel.SetActive(false);
        requestPanel.SetActive(true);
    }

    public void OpenOfferPanel()
    {
        mainTradePanel.SetActive(false);
        requestPanel.SetActive(false);
        offerPanel.SetActive(true);
    }

    public void OpenMainTradePanel()
    {
        requestPanel.SetActive(false);
        offerPanel.SetActive(false);
        mainTradePanel.SetActive(true);
    }

    public void ApplyRequestSelection()
    {
        OnApplyClicked(requestPanel, requestResourceButtons, selectedRequestedResources, selectedRequestedQuantities);
    }

    public void ApplyOfferSelection()
    {
        OnApplyClicked(offerPanel, offerResourceButtons, selectedOfferedResources, selectedOfferedQuantities);
    }

}
