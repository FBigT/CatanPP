using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Assets.Scripts.Models;
using Assets.Scripts.DevCards.Core;



public class DevCardPanelController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject devCardPanel;
    public Button buyDevCardButton;
    public Transform cardContainer;
    public GameObject cardItemPrefab;
    
    [Header("Card Icons")]
    public Sprite knightIcon;
    public Sprite victoryPointIcon;
    public Sprite roadBuildingIcon;
    public Sprite yearOfPlentyIcon;

    private List<GameObject> cardItems = new List<GameObject>();

    private void Start()
    {
        // Subscribe to DevCardManager events
        if (DevCardManager.Instance != null)
        {
            DevCardManager.Instance.OnCardsUpdated += UpdateCardDisplay;
            DevCardManager.Instance.OnError += OnError;
        }

        // Setup buy button
        if (buyDevCardButton != null)
            buyDevCardButton.onClick.AddListener(() => DevCardManager.Instance?.BuyDevCard());

        // Initially hide panel
        if (devCardPanel != null)
            devCardPanel.SetActive(false);
    }

    private void UpdateCardDisplay(List<DevCardDto> cards)
    {
        // Clear existing items
        foreach (var item in cardItems)
        {
            if (item != null)
                Destroy(item);
        }
        cardItems.Clear();

        if (cards.Count == 0)
        {
            if (devCardPanel != null)
                devCardPanel.SetActive(false);
            return;
        }

        if (devCardPanel != null)
            devCardPanel.SetActive(true);

        // Create UI items for each card type
        var cardCounts = new Dictionary<DevCardType, int>();
        foreach (var card in cards)
        {
            if (cardCounts.ContainsKey(card.type))
                cardCounts[card.type]++;
            else
                cardCounts[card.type] = 1;
        }

        foreach (var kvp in cardCounts)
        {
            CreateCardItem(kvp.Key, kvp.Value, cards.Find(c => c.type == kvp.Key));
        }
    }

    private void CreateCardItem(DevCardType type, int count, DevCardDto cardData)
    {
        if (cardItemPrefab == null || cardContainer == null) return;

        GameObject item = Instantiate(cardItemPrefab, cardContainer);
        cardItems.Add(item);

        // Setup the item (assuming it has these components)
        var icon = item.GetComponentInChildren<Image>();
        var countText = item.GetComponentInChildren<TMP_Text>();
        var playButton = item.GetComponentInChildren<Button>();

        // Set icon
        if (icon != null)
            icon.sprite = GetIconForType(type);

        // Set count text
        if (countText != null)
            countText.text = $"{type.ToString().Replace("_", " ")} x{count}";

        // Set button - Fixed method call
        if (playButton != null)
        {
            playButton.interactable = cardData.playable && !cardData.used;
            playButton.onClick.AddListener(() => {
                // Use the correct method signature
                DevCardManager.Instance?.PlayDevCard(cardData, type);
                // OnCardPlayed is now called automatically inside PlayDevCard
            });
        }
    }

    private Sprite GetIconForType(DevCardType type)
    {
        switch (type)
        {
            case DevCardType.KNIGHT: return knightIcon;
            case DevCardType.VICTORY_POINT: return victoryPointIcon;
            case DevCardType.ROAD_BUILDING: return roadBuildingIcon;
            case DevCardType.YEAR_OF_PLENTY: return yearOfPlentyIcon;
            default: return null;
        }
    }

    private void OnError(string error)
    {
        Debug.LogError("Dev Card Error: " + error);
    }
}
