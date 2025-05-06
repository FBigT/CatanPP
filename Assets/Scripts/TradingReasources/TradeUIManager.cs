using Assets.Scripts.TradingReasources;
using Assets.Scripts.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.TradingReasources
{
    public class TradeUIManager : MonoBehaviour
    {
        [Header("API Client")]
        public TradeApiClient apiClient;

        [Header("Counterparty Selection (Bank must be option 0)")]
        public Dropdown counterpartyDropdown;

        [Header("Screen Manager")]
        public TradeScreenManager screenMgr;

        [Header("Bank-only Fields")]
        public TMP_InputField portTypeField;
        public TMP_InputField portRatioField;

        [Header("Confirmation UI (drag both!)")]
        public GameObject confirmPanel;
        public TMP_Text confirmText;  

        const string BANK = "Bank";
        string selectedCounterparty = BANK;

        long sessionId;
        string myUsername;


        void OnEnable() => RefreshCounterparties();

        public void RefreshCounterparties() => StartCoroutine(FillNames());

        IEnumerator FillNames()
        {
            string prev = counterpartyDropdown.options.Count > 0
                ? counterpartyDropdown.options[counterpartyDropdown.value].text
                : BANK;

            counterpartyDropdown.ClearOptions();
            counterpartyDropdown.AddOptions(new List<string> { BANK });
            counterpartyDropdown.SetValueWithoutNotify(0);
            selectedCounterparty = BANK;

            if (apiClient == null) yield break;

            bool done = false;
            apiClient.GetSessionPlayers(sessionId, (list, _) =>
            {
                var names = new List<string> { BANK };
                foreach (var p in list)
                    if (!p.username.Equals(myUsername, StringComparison.OrdinalIgnoreCase))
                        names.Add(p.username);

                counterpartyDropdown.ClearOptions();
                counterpartyDropdown.AddOptions(names);

                int idx = names.IndexOf(prev);
                if (idx < 0) idx = 0;
                counterpartyDropdown.SetValueWithoutNotify(idx);
                selectedCounterparty = names[idx];

                counterpartyDropdown.onValueChanged.RemoveAllListeners();
                counterpartyDropdown.onValueChanged.AddListener(i => selectedCounterparty = names[i]);

                done = true;
            });
            while (!done) yield return null;
        }

        public void OnApplyTrade()
        {
            if (selectedCounterparty == BANK)
                SendBankTrade();
            else
                SendPlayerTrade();
        }

        void SendPlayerTrade()
        {
            var dto = new PlayerTradeDto
            {
                sessionId = sessionId,
                fromUser = myUsername,
                toUser = selectedCounterparty,
                offered = screenMgr.OfferedGroup,
                requested = screenMgr.RequestedGroup
            };

            apiClient.TradePlayer(dto, (ok, _) =>
                ShowConfirm(ok ? "Trade sent!" : "Trade not sent"));
        }

        void SendBankTrade()
        {
            var dto = new BankTradeDto
            {
                sessionId = sessionId,
                fromUser = myUsername,
                offered = screenMgr.OfferedGroup,
                requested = screenMgr.RequestedGroup,
                portType = portTypeField ? portTypeField.text : string.Empty,
                portRatio = portRatioField && int.TryParse(portRatioField.text, out var r) ? r : 0
            };

            apiClient.TradeBank(dto, (ok, _) =>
                ShowConfirm(ok ? "Trade sent!" : "Trade not sent"));
        }

        void ShowConfirm(string msg)
        {
            Debug.Log("ShowConfirm called → " + msg); 

            if (confirmText) confirmText.text = msg;
            if (confirmPanel) confirmPanel.SetActive(true);
        }

        public void HideConfirm()                       
        {
            if (confirmPanel) confirmPanel.SetActive(false);
        }
    }
}
