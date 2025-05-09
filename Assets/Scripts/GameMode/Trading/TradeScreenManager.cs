using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Assets.Scripts.Utils;
using Assets.Scripts.GameMode.Trading.Models;
using Assets.Scripts.GameMode.Trading;   // ← keep only this TradingManager

namespace Assets.Scripts.GameMode.Trading
{
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
        public Dropdown playerDropdown;

        readonly List<string> selectedRequestedResources = new();
        readonly List<int> selectedRequestedQuantities = new();
        readonly List<string> selectedOfferedResources = new();
        readonly List<int> selectedOfferedQuantities = new();

        long sessionId;
        string currentUserName;

        void Start()
        {
            sessionId = LocalStorageService.GetInt("session-id") ?? 0;
            currentUserName = LocalStorageService.GetString("username") ?? "";

            playerDropdown.ClearOptions();
            TradingManager.Instance.GetSessionPlayers(
                sessionId,
                OnPlayersLoaded,
                err => Debug.LogError($"[TradeScreen] Load players failed: {err}")
            );
        }

        void OnPlayersLoaded(List<SessionPlayerDto> players)
        {
            var options = new List<string> { "Bank" };
            options.AddRange(players.Select(p => p.username));

            playerDropdown.ClearOptions();
            playerDropdown.AddOptions(options);
            playerDropdown.value = 0;
            playerDropdown.RefreshShownValue();
        }

        public void OpenRequestPanel() => SwitchPanel(requestPanel);
        public void OpenOfferPanel() => SwitchPanel(offerPanel);
        public void OpenMainTradePanel() => SwitchPanel(mainTradePanel);

        void SwitchPanel(GameObject panel)
        {
            mainTradePanel.SetActive(panel == mainTradePanel);
            requestPanel.SetActive(panel == requestPanel);
            offerPanel.SetActive(panel == offerPanel);
            EventSystem.current.SetSelectedGameObject(null);
        }

        public void ApplyRequestSelection() =>
            CaptureSelections(requestResourceButtons,
                              selectedRequestedResources,
                              selectedRequestedQuantities);

        public void ApplyOfferSelection() =>
            CaptureSelections(offerResourceButtons,
                              selectedOfferedResources,
                              selectedOfferedQuantities);

        void CaptureSelections(List<ResourceButtonHandler> buttons,
                               List<string> names,
                               List<int> qty)
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
        }

        public void OnApplyTradeClicked()
        {
            string toUser = playerDropdown.options[playerDropdown.value].text;

            if (toUser == "Bank")
            {
                var dto = new BankTradeDto
                {
                    sessionId = sessionId,
                    fromUser = currentUserName,
                    offered = ResourceGroup.FromLists(selectedOfferedResources, selectedOfferedQuantities),
                    requested = ResourceGroup.FromLists(selectedRequestedResources, selectedRequestedQuantities)
                };
                TradingManager.Instance.TradeWithBank(dto,
                    () => Debug.Log("[TradeScreen] Bank trade successful"),
                    err => Debug.LogError("[TradeScreen] Bank trade failed: " + err));
            }
            else
            {
                var dto = new PlayerTradeDto
                {
                    sessionId = sessionId,
                    fromUser = currentUserName,
                    toUser = toUser,
                    offered = ResourceGroup.FromLists(selectedOfferedResources, selectedOfferedQuantities),
                    requested = ResourceGroup.FromLists(selectedRequestedResources, selectedRequestedQuantities)
                };
                TradingManager.Instance.TradeWithPlayer(dto,
                    () => Debug.Log("[TradeScreen] Trade successful"),
                    err => Debug.LogError("[TradeScreen] Trade failed: " + err));
            }
        }
    }
}
