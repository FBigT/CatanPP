using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Assets.Scripts.TradingResources;
using Assets.Scripts.Utils;
using Assets.Scripts.TradingReasources.Models;

public class TradeScreenManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainTradePanel;
    public GameObject requestPanel;
    public GameObject offerPanel;

    [Header("Resource Buttons")]
    public List<ResourceButtonHandler> requestResourceButtons;
    public List<ResourceButtonHandler> offerResourceButtons;

    [Header("Player Dropdown")]
    public Dropdown playerDropdown;  // UnityEngine.UI.Dropdown

    // selections
    public List<string> selectedRequestedResources = new();
    public List<int> selectedRequestedQuantities = new();
    public List<string> selectedOfferedResources = new();
    public List<int> selectedOfferedQuantities = new();

    // context
    private long sessionId;
    private string currentUserName;

    void Start()
    {
        // grab what should already have been saved
        sessionId = LocalStorageService.GetInt("session-id") ?? 0;
        currentUserName = LocalStorageService.GetString("username") ?? "";

        Debug.Log($"[TradeScreen] sessionId = {sessionId}, user = {currentUserName}");

        // clear any placeholder options
        playerDropdown.ClearOptions();

        TradingManager.Instance.GetSessionPlayers(
            sessionId,
            OnPlayersLoaded,
            err => Debug.LogError($"[TradeScreen] Load players failed: {err}")
        );
    }

    private void OnPlayersLoaded(List<SessionPlayerDto> players)
    {
        // build a list: [ "Bank", other real players... ]
        var otherNames = players
            .Where(p => p.username != currentUserName && p.active && !p.isAi)
            .Select(p => p.username)
            .ToList();

        // always put Bank at [0]
        var dropdownOptions = new List<string> { "Bank" };
        if (otherNames.Count > 0)
            dropdownOptions.AddRange(otherNames);
        else
            dropdownOptions.Add("No other players");

        Debug.Log($"[TradeScreen] Bank + players: {string.Join(", ", dropdownOptions)}");

        playerDropdown.ClearOptions();
        playerDropdown.AddOptions(dropdownOptions);
        playerDropdown.value = 0;              // default-select Bank
        playerDropdown.RefreshShownValue();    // update UI immediately
    }

    // panel nav
    public void OpenRequestPanel() => SwitchPanel(requestPanel);
    public void OpenOfferPanel() => SwitchPanel(offerPanel);
    public void OpenMainTradePanel() => SwitchPanel(mainTradePanel);

    private void SwitchPanel(GameObject panel)
    {
        mainTradePanel.SetActive(panel == mainTradePanel);
        requestPanel.SetActive(panel == requestPanel);
        offerPanel.SetActive(panel == offerPanel);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void ApplyRequestSelection() =>
        CaptureSelections(requestResourceButtons, selectedRequestedResources, selectedRequestedQuantities);

    public void ApplyOfferSelection() =>
        CaptureSelections(offerResourceButtons, selectedOfferedResources, selectedOfferedQuantities);

    private void CaptureSelections(List<ResourceButtonHandler> buttons, List<string> names, List<int> qty)
    {
        names.Clear();
        qty.Clear();
        foreach (var b in buttons)
            if (b.GetQuantity() > 0)
            {
                names.Add(b.resourceName);
                qty.Add(b.GetQuantity());
            }
    }

    // send the trade
    public void OnApplyTradeClicked()
    {
        var toUser = playerDropdown.options[playerDropdown.value].text;

        if (toUser == "Bank")
        {
            var bankDto = new BankTradeDto
            {
                sessionId = sessionId,
                fromUser = currentUserName,
                offered = ResourceGroup.FromLists(selectedOfferedResources, selectedOfferedQuantities),
                requested = ResourceGroup.FromLists(selectedRequestedResources, selectedRequestedQuantities)
            };
            TradingManager.Instance.TradeWithBank(
                bankDto,
                () => Debug.Log("[TradeScreen] Bank trade successful"),
                err => Debug.LogError("[TradeScreen] Bank trade failed: " + err)
            );
        }
        else
        {
            var playerDto = new PlayerTradeDto
            {
                sessionId = sessionId,
                fromUser = currentUserName,
                toUser = toUser,
                offered = ResourceGroup.FromLists(selectedOfferedResources, selectedOfferedQuantities),
                requested = ResourceGroup.FromLists(selectedRequestedResources, selectedRequestedQuantities)
            };
            TradingManager.Instance.TradeWithPlayer(
                playerDto,
                () => Debug.Log("[TradeScreen] Trade successful"),
                err => Debug.LogError("[TradeScreen] Trade failed: " + err)
            );
        }
    }
}
