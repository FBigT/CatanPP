using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Enums;
using Catan.GameMode;
using Catan.Managers;
using Assets.Scripts.DevCards.UI;

namespace Catan.UI.LeftMenu
{
    public class OnStructureTabEvents : MonoBehaviour
    {
        [Header("Structure Buttons")]
        public Button buyRoadButton;
        public Button buySettlementButton;
        public Button buyCityButton;
        public Button devCardsButton; // This is your "Dev Cards" toggle button

        [Header("Structure Prefabs")]
        public GameObject roadPrefab;
        public GameObject settlementPrefab;
        public GameObject cityPrefab;

        private void Start()
        {
            // Subscribe to game mode events
            if (CampaignGameMode.Instance != null)
            {
                CampaignGameMode.Instance.TurnChangedForUI += OnTurnChanged;
            }

            // Set up button click handlers
            SetupButtonHandlers();
        }

        private void SetupButtonHandlers()
        {
            if (buyRoadButton != null)
            {
                buyRoadButton.onClick.AddListener(() => PurchaseManager.Instance.SetPurchase(PurchaseType.Road));
            }

            if (buySettlementButton != null)
            {
                buySettlementButton.onClick.AddListener(() => PurchaseManager.Instance.SetPurchase(PurchaseType.Settlement));
            }

            if (buyCityButton != null)
            {
                buyCityButton.onClick.AddListener(() => PurchaseManager.Instance.SetPurchase(PurchaseType.City));
            }

            if (devCardsButton != null)
            {
                devCardsButton.onClick.AddListener(() => PurchaseManager.Instance.SetPurchase(PurchaseType.DevCard));
            }
        }

        private void OnTurnChanged(bool isMyTurn, bool inSetupPhase)
        {
            // Update button availability based on turn and setup phase
            if (!isMyTurn)
            {
                // Not player's turn - disable all buttons
                DisableAllButtons();
                return;
            }

            if (inSetupPhase)
            {
                // Setup phase - only allow what the game mode permits
                // Dev Cards are not available in setup phase
                if (devCardsButton != null)
                    devCardsButton.interactable = false;

                // Other buttons will be managed by PurchaseManager during setup
                return;
            }

            // Normal play phase - check affordability
            var currentPlayer = CampaignGameMode.Instance?.CurrentPlayer;
            if (currentPlayer != null)
            {
                UpdateAffordability(currentPlayer.Resources);
            }
        }

        public void UpdateAffordability(int[] playerResources)
        {
            // Check affordability for structure buttons
            if (buyRoadButton != null)
            {
                buyRoadButton.interactable = CanAfford(PurchaseType.Road, playerResources);
            }

            if (buySettlementButton != null)
            {
                buySettlementButton.interactable = CanAfford(PurchaseType.Settlement, playerResources);
            }

            if (buyCityButton != null)
            {
                buyCityButton.interactable = CanAfford(PurchaseType.City, playerResources);
            }

            // FORCE ENABLE Dev Cards button - ALWAYS INTERACTABLE
            if (devCardsButton != null)
            {
                devCardsButton.interactable = true; // FORCE ENABLE
                Debug.Log("Dev Cards button force enabled!");
            }
        }


        private bool CanAfford(PurchaseType purchaseType, int[] playerResources)
        {
            if (playerResources == null) return false;

            int[] cost = Costs.Get(purchaseType);
            for (int i = 0; i < cost.Length && i < playerResources.Length; i++)
            {
                if (playerResources[i] < cost[i])
                {
                    return false;
                }
            }
            return true;
        }

        private void DisableAllButtons()
        {
            if (buyRoadButton != null) buyRoadButton.interactable = false;
            if (buySettlementButton != null) buySettlementButton.interactable = false;
            if (buyCityButton != null) buyCityButton.interactable = false;
            if (devCardsButton != null) devCardsButton.interactable = false;
        }

        public GameObject GetSelectedStructure()
        {
            PurchaseType selected = PurchaseManager.Instance.Selected;

            return selected switch
            {
                PurchaseType.Road => roadPrefab,
                PurchaseType.Settlement => settlementPrefab,
                PurchaseType.City => cityPrefab,
                _ => null
            };
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (CampaignGameMode.Instance != null)
            {
                CampaignGameMode.Instance.TurnChangedForUI -= OnTurnChanged;
            }
        }
    }
}
