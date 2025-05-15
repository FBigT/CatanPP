using UnityEngine;
using UnityEngine.UIElements;
using Assets.Scripts.Enums;   // PurchaseType
using Catan.Managers;        // PurchaseManager
using Catan.UI;              // DevCardPanelController

namespace Catan.UI
{
    public class LeftMenuUI : MonoBehaviour
    {
        private Button btnRoad, btnSettle, btnCity, btnDevCard, btnShowDevCards;
        private DevCardPanelController _devCardPanel;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>()?.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("LeftMenuUI: missing UIDocument");
                enabled = false;
                return;
            }

            // Find the DevCardPanelController (even if panel GameObject is inactive)
            _devCardPanel = FindObjectOfType<DevCardPanelController>(true);
            if (_devCardPanel == null)
                Debug.LogError("DevCardPanelController not found! Make sure DevCardPanel is in your scene.");

            // Grab your buttons from the UXML
            btnRoad = root.Q<Button>("BuyRoadButton");
            btnSettle = root.Q<Button>("BuySettlementButton");
            btnCity = root.Q<Button>("BuyCityButton");
            btnDevCard = root.Q<Button>("BuyDevCardButton");
            btnShowDevCards = root.Q<Button>("ShowDevCardsButton");  // your toggle button

            // Wire up structure buys as before
            if (btnRoad != null) btnRoad.clicked += () => PurchaseManager.Instance.SetPurchase(PurchaseType.Road);
            if (btnSettle != null) btnSettle.clicked += () => PurchaseManager.Instance.SetPurchase(PurchaseType.Settlement);
            if (btnCity != null) btnCity.clicked += () => PurchaseManager.Instance.SetPurchase(PurchaseType.City);

            // Buy DevCard only triggers the backend buy & list refresh
            if (btnDevCard != null)
                btnDevCard.clicked += () => _devCardPanel.BuyAndRefresh();

            // Show/Hide the DevCard panel
            if (btnShowDevCards != null)
            {
                btnShowDevCards.clicked += () =>
                {
                    if (_devCardPanel.gameObject.activeSelf)
                        _devCardPanel.gameObject.SetActive(false);
                    else
                        _devCardPanel.Open();
                };
            }
            else
            {
                Debug.LogWarning("ShowDevCardsButton not found in UXML. Add a <Button name=\"ShowDevCardsButton\"> to your LeftMenu UXML.");
            }
        }
    }
}
