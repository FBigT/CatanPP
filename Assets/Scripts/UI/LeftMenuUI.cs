using UnityEngine;
using UnityEngine.UIElements;
using Assets.Scripts.Enums;
using Catan.Managers;
using Assets.Scripts.Utils;

namespace Catan.UI
{
    public class LeftMenuUI : MonoBehaviour
    {
        private Button btnRoad, btnSettle, btnCity, btnDevCard;

        void OnEnable()
        {
            var root = GetComponent<UIDocument>()?.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("LeftMenuUI: missing UIDocument");
                enabled = false;
                return;
            }

            btnRoad = root.Q<Button>("BuyRoadButton");
            btnSettle = root.Q<Button>("BuySettlementButton");
            btnCity = root.Q<Button>("BuyCityButton");
            btnDevCard = root.Q<Button>("BuyDevCardButton");

            if (btnRoad != null) btnRoad.clicked += () => PurchaseManager.Instance.SetPurchase(PurchaseType.Road);
            if (btnSettle != null) btnSettle.clicked += () => PurchaseManager.Instance.SetPurchase(PurchaseType.Settlement);
            if (btnCity != null) btnCity.clicked += () => PurchaseManager.Instance.SetPurchase(PurchaseType.City);
            if (btnDevCard != null) btnDevCard.clicked += () => PurchaseManager.Instance.SetPurchase(PurchaseType.DevCard);
        }
    }
}