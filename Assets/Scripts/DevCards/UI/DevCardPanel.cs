using UnityEngine;
using UnityEngine.UIElements;
using Assets.Scripts.Models;
using Assets.Scripts.DevCards.Core;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;

namespace Assets.Scripts.DevCards.UI
{
    public class DevCardPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private UIDocument uiDocument;

        [Header("Card Icons")]
        [SerializeField] private Sprite knightIcon;
        [SerializeField] private Sprite victoryPointIcon;
        [SerializeField] private Sprite roadBuildingIcon;
        [SerializeField] private Sprite yearOfPlentyIcon;

        [Header("Card Template")]
        [SerializeField] private VisualTreeAsset cardItemTemplate;

        // UI Elements
        private VisualElement panelRoot;
        private ScrollView cardScrollView;
        private Button buyCardButton;

        // Card management - SIMPLIFIED
        private List<DevCardDto> playerCards = new List<DevCardDto>();
        private List<VisualElement> cardElements = new List<VisualElement>();

        // Dependencies
        private DevCardManager devCardManager;

        // State tracking
        public bool IsVisible { get; private set; } = false;

        private void Awake()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
        }

        private void Start()
        {
            InitializeReferences();
            InitializeUI();
        }

        private void InitializeReferences()
        {
            devCardManager = DevCardManager.Instance;
            if (devCardManager == null)
            {
                Debug.LogError("DevCardManager instance not found!");
                return;
            }

            // Subscribe to events
            devCardManager.OnCardsUpdated += OnPlayerCardsUpdated;
            devCardManager.OnCardBought += OnCardBought;
            devCardManager.OnError += OnError;
        }

        private void InitializeUI()
        {
            if (uiDocument?.rootVisualElement == null)
            {
                Debug.LogError("UIDocument or root element is null!");
                return;
            }

            var root = uiDocument.rootVisualElement;

            // Find UI elements
            panelRoot = root.Q<VisualElement>("DevCardPanel");
            cardScrollView = root.Q<ScrollView>("DevCardScroll");

            if (cardScrollView == null)
            {
                Debug.LogError("Could not find ScrollView with name 'DevCardScroll'!");
                return;
            }

            // Configure ScrollView properties
            cardScrollView.mode = ScrollViewMode.Vertical;
            cardScrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;
            cardScrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

            // Setup buy button
            buyCardButton = root.Q<Button>("BuyCardButton");
            if (buyCardButton != null)
            {
                buyCardButton.clicked += BuyDevCard;
            }

            // Setup close button
            var closeButton = root.Q<Button>("CloseButton");
            if (closeButton != null)
            {
                closeButton.clicked += HidePanel;
            }

            // Start hidden
            SetPanelVisibility(false);

            Debug.Log("DevCard UI initialized successfully");
        }

        private void OnPlayerCardsUpdated(List<DevCardDto> cards)
        {
            Debug.Log($"=== CARDS UPDATED: {cards.Count} cards ===");
            foreach (var card in cards)
            {
                Debug.Log($"Card: {card.type}, Playable: {card.playable}, Used: {card.used}");
            }

            playerCards = new List<DevCardDto>(cards);

            // Force a frame delay to ensure WebSocket updates are complete
            StartCoroutine(DelayedRefresh());
        }

        private IEnumerator DelayedRefresh()
        {
            yield return null; // Wait one frame
            RefreshCardDisplay();
        }
        [ContextMenu("Debug Button States")]
        public void DebugButtonStates()
        {
            Debug.Log("=== BUTTON STATE DEBUG ===");

            var buttons = cardScrollView.Query<Button>().ToList();
            for (int i = 0; i < buttons.Count && i < playerCards.Count; i++)
            {
                var button = buttons[i];
                var card = playerCards[i];

                Debug.Log($"Card {i}: {card.type} (playable: {card.playable}, used: {card.used})");
                Debug.Log($"  Button enabled: {button.enabledSelf}");
                Debug.Log($"  Button text: '{button.text}'");
                Debug.Log($"  Button style display: {button.style.display.value}");
            }
        }


        private void OnCardBought(string message)
        {
            Debug.Log("Card bought: " + message);
            // Don't refresh here - wait for OnPlayerCardsUpdated
        }

        private void OnError(string error)
        {
            Debug.LogError("Dev Card Error: " + error);
        }

        private void RefreshCardDisplay()
        {
            // Clear existing cards
            ClearCardDisplay();

            if (playerCards.Count == 0)
            {
                // Show empty state
                var emptyLabel = new Label("No development cards available");
                emptyLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
                emptyLabel.style.fontSize = 14;
                emptyLabel.style.marginTop = 20;
                cardScrollView?.Add(emptyLabel);
                cardElements.Add(emptyLabel);
                return;
            }

            // Create individual card displays for EACH card
            Debug.Log($"Creating UI for {playerCards.Count} individual cards");

            foreach (var card in playerCards)
            {
                CreateIndividualCardDisplay(card);
            }

            // Force layout update
            cardScrollView?.MarkDirtyRepaint();
        }

        private void CreateIndividualCardDisplay(DevCardDto card)
        {
            if (cardItemTemplate == null)
            {
                Debug.LogWarning("Cannot create card display: missing template");
                return;
            }

            // Instantiate the template
            TemplateContainer cardInstance = cardItemTemplate.Instantiate();

            // Configure the template container
            cardInstance.style.flexGrow = 1;
            cardInstance.style.flexShrink = 0;

            // Query elements
            var cardIcon = cardInstance.Q<VisualElement>(className: "card-icon") ??
                           cardInstance.Q<VisualElement>("CardIcon");

            var titleLabel = cardInstance.Q<Label>(className: "Title") ??
                             cardInstance.Q<Label>("CardTitle");

            var descLabel = cardInstance.Q<Label>(className: "Desc") ??
                            cardInstance.Q<Label>("CardDescription");

            var playButton = cardInstance.Q<Button>("PlayButton") ??
                             cardInstance.Q<Button>();

            // Verify essential elements exist
            if (cardIcon == null || titleLabel == null || playButton == null)
            {
                Debug.LogError($"Failed to find required elements in template for {card.type}");
                return;
            }

            // Set up card icon
            if (cardIcon != null)
            {
                var icon = GetIconForType(card.type);
                if (icon != null)
                {
                    cardIcon.style.backgroundImage = new StyleBackground(icon);
                    cardIcon.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                }
            }

            // Set up text content
            if (titleLabel != null)
            {
                titleLabel.text = GetCardTitle(card.type);
            }

            if (descLabel != null)
            {
                descLabel.text = GetCardDescription(card.type);
            }

            // Set up play button - THIS IS THE KEY FIX
            SetupPlayButton(playButton, card);

            // Add to scroll view
            cardScrollView?.Add(cardInstance);
            cardElements.Add(cardInstance);

            Debug.Log($"Created display for {card.type} (ID: {card.id}, playable: {card.playable})");
        }

        private void SetupPlayButton(Button playButton, DevCardDto card)
        {
            if (playButton == null) return;

            // Enable/disable based on current card state
            bool isPlayable = card.playable && !card.used;

            // Force a state reset before applying new state
            playButton.SetEnabled(true);
            playButton.SetEnabled(isPlayable);

            // Set button text
            playButton.text = isPlayable ? "PLAY" : "LOCKED";

            // Clear existing handlers using Clickable property instead
            playButton.clickable = new Clickable(() => { });

            if (isPlayable)
            {
                playButton.clickable = new Clickable(() => {
                    PlayCard(card, card.type);
                });
            }

            Debug.Log($"Setup button for {card.type} (ID: {card.id}): enabled={isPlayable}, button.enabledSelf={playButton.enabledSelf}");
        }

        private void ForceButtonRefresh(Button button)
        {
            if (button == null) return;

            // Force a layout recalculation
            button.MarkDirtyRepaint();

            // Trigger a state update by temporarily toggling enabled state
            bool currentState = button.enabledSelf;
            button.SetEnabled(!currentState);
            button.SetEnabled(currentState);
        }



        private string GetCardTitle(DevCardType type)
        {
            return type switch
            {
                DevCardType.KNIGHT => "Knight",
                DevCardType.VICTORY_POINT => "Victory Point",
                DevCardType.ROAD_BUILDING => "Road Building",
                DevCardType.YEAR_OF_PLENTY => "Year of Plenty",
                _ => type.ToString()
            };
        }

        private string GetCardDescription(DevCardType type)
        {
            return type switch
            {
                DevCardType.KNIGHT => "Move robber and steal from player",
                DevCardType.VICTORY_POINT => "Instant victory point",
                DevCardType.ROAD_BUILDING => "Place 2 free roads",
                DevCardType.YEAR_OF_PLENTY => "Choose 2 free resources",
                _ => "Development card"
            };
        }

        private Sprite GetIconForType(DevCardType type)
        {
            return type switch
            {
                DevCardType.KNIGHT => knightIcon,
                DevCardType.VICTORY_POINT => victoryPointIcon,
                DevCardType.ROAD_BUILDING => roadBuildingIcon,
                DevCardType.YEAR_OF_PLENTY => yearOfPlentyIcon,
                _ => null
            };
        }

        private void PlayCard(DevCardDto card, DevCardType type)
        {
            Debug.Log($"Playing card: {type} (ID: {card.id})");
            if (devCardManager != null)
            {
                devCardManager.PlayDevCard(card, type);
            }
        }

        private void BuyDevCard()
        {
            devCardManager?.BuyDevCard();
        }

        private void ClearCardDisplay()
        {
            if (cardScrollView != null)
            {
                cardScrollView.Clear();
            }
            cardElements.Clear();
        }

        private void SetPanelVisibility(bool visible)
        {
            if (panelRoot != null)
            {
                panelRoot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                IsVisible = visible;
                Debug.Log($"DevCard panel visibility set to: {visible}");
            }
        }

        public void ShowPanel()
        {
            SetPanelVisibility(true);
        }

        public void HidePanel()
        {
            SetPanelVisibility(false);
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (devCardManager != null)
            {
                devCardManager.OnCardsUpdated -= OnPlayerCardsUpdated;
                devCardManager.OnCardBought -= OnCardBought;
                devCardManager.OnError -= OnError;
            }

            // Clean up UI event handlers
            if (buyCardButton != null)
            {
                buyCardButton.clicked -= BuyDevCard;
            }

            ClearCardDisplay();
        }
    }
}
