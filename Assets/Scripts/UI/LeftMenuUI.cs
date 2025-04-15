using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using Assets.Scripts.Enums;    // PurchaseType
using Assets.Scripts.Managers; // PurchaseManager
using Assets.Scripts.Utils;    // EndpointUtils, LocalStorageService

public class LeftMenuUI : MonoBehaviour
{
    private Button buyRoadButton, buySettlementButton, buyCityButton, buyDevCardButton;
    private VisualElement devCardPanel;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        buyRoadButton = root.Q<Button>("BuyRoadButton");
        buySettlementButton = root.Q<Button>("BuySettlementButton");
        buyCityButton = root.Q<Button>("BuyCityButton");
        buyDevCardButton = root.Q<Button>("BuyDevCardButton");
        devCardPanel = root.Q<VisualElement>("DevCardPanel");

        if (buyRoadButton != null)
            buyRoadButton.clicked += OnBuyRoadClicked;
        if (buySettlementButton != null)
            buySettlementButton.clicked += OnBuySettlementClicked;
        if (buyCityButton != null)
            buyCityButton.clicked += OnBuyCityClicked;
        if (buyDevCardButton != null)
            buyDevCardButton.clicked += OnBuyDevCardClicked;
    }

    private void OnBuyRoadClicked()
    {
        Debug.Log("Buy Road clicked!");
        // Step 1: Just set PurchaseType to Road; 
        // Step 2: The user will click an edge on the board => actual placement happens
        PurchaseManager.Instance.SetPurchase(PurchaseType.Road);
    }

    private void OnBuySettlementClicked()
    {
        Debug.Log("Buy Settlement clicked!");
        // Step 1: Mark we want to place a settlement
        PurchaseManager.Instance.SetPurchase(PurchaseType.Settlement);
    }

    private void OnBuyCityClicked()
    {
        Debug.Log("Buy City clicked!");
        // If you want "upgrade city" in two steps, do something similar. 
        // Alternatively, you might do an immediate request if the user doesn't need to pick a location. 
        PurchaseManager.Instance.SetPurchase(PurchaseType.City);
    }

    private void OnBuyDevCardClicked()
    {
        Debug.Log("Buy Dev Card clicked!");
        // For dev cards, you'd presumably do an immediate request 
        // that doesn't require a tile or corner
        PurchaseManager.Instance.SetPurchase(PurchaseType.DevCard);
        // Then you can do a direct POST "Buy Dev Card" if you want, or let the user pick from a list, etc.
    }
}
