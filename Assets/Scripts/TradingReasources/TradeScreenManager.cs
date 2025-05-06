using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Assets.Scripts.TradingReasources;

public class TradeScreenManager : MonoBehaviour
{
    public GameObject mainTradePanel;
    public GameObject requestPanel;
    public GameObject offerPanel;

    public List<ResourceButtonHandler> requestResourceButtons;
    public List<ResourceButtonHandler> offerResourceButtons;

    public List<string> selectedRequestedResources = new();
    public List<int> selectedRequestedQuantities = new();

    public List<string> selectedOfferedResources = new();
    public List<int> selectedOfferedQuantities = new();

    public ResourceGroup OfferedGroup { get; private set; } = new();
    public ResourceGroup RequestedGroup { get; private set; } = new();

    static readonly Dictionary<string, System.Action<ResourceGroup, int>> MAP =
        new()
        {
            { "brick"  , (g,q) => g.brick   = q },
            { "crystal", (g,q) => g.crystal = q },
            { "ore"    , (g,q) => g.ore     = q },
            { "rice"   , (g,q) => g.rice    = q },
            { "sheep"  , (g,q) => g.sheep   = q },
            { "silver" , (g,q) => g.silver  = q },
            { "gold"   , (g,q) => g.gold    = q },
            { "wood"   , (g,q) => g.wood    = q }
        };

    void FillGroup(ResourceGroup g, List<string> names, List<int> qty)
    {
        foreach (var key in MAP.Keys) MAP[key](g, 0);
        for (int i = 0; i < names.Count; i++)
        {
            string n = names[i].ToLowerInvariant();
            if (MAP.TryGetValue(n, out var set)) set(g, qty[i]);
        }
    }

    public void OnCancelClicked(GameObject panel)
    {
        panel.SetActive(false);
        mainTradePanel.SetActive(true);
    }

    public void OnApplyClicked(GameObject panel, List<ResourceButtonHandler> buttons,
                               List<string> names, List<int> qty)
    {
        names.Clear();
        qty.Clear();

        foreach (var b in buttons)
        {
            int q = b.GetQuantity();
            if (q > 0)
            {
                names.Add(b.resourceName);
                qty.Add(q);
            }
        }

        EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(SwitchToMain(panel));
    }

    IEnumerator SwitchToMain(GameObject panel)
    {
        yield return new WaitForSeconds(0.1f);
        panel.SetActive(false);
        mainTradePanel.SetActive(true);
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
        OnApplyClicked(requestPanel, requestResourceButtons,
                       selectedRequestedResources, selectedRequestedQuantities);

        FillGroup(RequestedGroup, selectedRequestedResources, selectedRequestedQuantities);
    }

    public void ApplyOfferSelection()
    {
        OnApplyClicked(offerPanel, offerResourceButtons,
                       selectedOfferedResources, selectedOfferedQuantities);

        FillGroup(OfferedGroup, selectedOfferedResources, selectedOfferedQuantities);
    }
}
