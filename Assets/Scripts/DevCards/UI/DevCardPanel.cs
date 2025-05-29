using UnityEngine;
using UnityEngine.UIElements;
using Assets.Scripts.Models;
using Assets.Scripts.DevCards.Core;
using System.Collections.Generic;
using System.Linq;

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

        // UI Elements
        private VisualElement panelRoot;
        private ScrollView cardScrollView;
        private Button buyCardButton;

        // Card management
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
            if (uiDocument?.rootVisualElement == null) return;

            var root = uiDocument.rootVisualElement;

            panelRoot = root.Q<VisualElement>("DevCardPanel");
            cardScrollView = root.Q<ScrollView>("DevCardScroll");

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

            // START HIDDEN
            SetPanelVisibility(false);
        }

        public void OnPlayerCardsUpdated(List<DevCardDto> cards)
        {
            playerCards = new List<DevCardDto>(cards);
            RefreshCardDisplay();
        }

        private void OnCardBought(string message)
        {
            Debug.Log("Card bought: " + message);
            RefreshCardDisplay();
        }

        private void OnError(string error)
        {
            Debug.LogError("Dev Card Error: " + error);
        }

        private void RefreshCardDisplay()
        {
            ClearCardDisplay();

            // Group cards by type and create displays
            var groupedCards = playerCards.GroupBy(c => c.type).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var cardGroup in groupedCards)
            {
                CreateCardTypeDisplay(cardGroup.Key, cardGroup.Value);
            }
        }

        private void CreateCardTypeDisplay(DevCardType cardType, List<DevCardDto> cards)
        {
            if (cards.Count == 0) return;

            // Create a card container
            var cardContainer = new VisualElement();
            cardContainer.AddToClassList("CardRoot");

            // Create icon
            var cardIcon = new VisualElement();
            cardIcon.AddToClassList("card-icon");

            // Set background image based on card type
            var icon = GetIconForType(cardType);
            if (icon != null)
            {
                cardIcon.style.backgroundImage = new StyleBackground(icon);
            }

            // Create title
            var titleLabel = new Label(GetCardTitle(cardType));
            titleLabel.AddToClassList("Title");

            // Create description
            var descLabel = new Label(GetCardDescription(cardType));
            descLabel.AddToClassList("Desc");

            // Create count label if more than 1
            if (cards.Count > 1)
            {
                var countLabel = new Label($"x{cards.Count}");
                countLabel.style.position = Position.Absolute;
                countLabel.style.top = 5;
                countLabel.style.right = 5;
                countLabel.style.backgroundColor = Color.black;
                countLabel.style.color = Color.white;
                countLabel.style.paddingTop = 2;
                countLabel.style.paddingBottom = 2;
                countLabel.style.paddingLeft = 2;
                countLabel.style.paddingRight = 2;
                cardContainer.Add(countLabel);
            }

            // Create play button
            var playButton = new Button(() => PlayCard(cards[0], cardType));
            playButton.text = "Play";
            playButton.name = "PlayButton";

            // Check if card can be played
            var playableCard = cards.FirstOrDefault(c => c.playable && !c.used);
            playButton.SetEnabled(playableCard != null);

            // Add elements to container
            cardContainer.Add(cardIcon);
            cardContainer.Add(titleLabel);
            cardContainer.Add(descLabel);
            cardContainer.Add(playButton);

            // Add to scroll view
            cardScrollView?.Add(cardContainer);
            cardElements.Add(cardContainer);
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
            cardScrollView?.Clear();
            cardElements.Clear();
        }

        private void SetPanelVisibility(bool visible)
        {
            if (panelRoot != null)
            {
                panelRoot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                IsVisible = visible;
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

            if (buyCardButton != null)
            {
                buyCardButton.clicked -= BuyDevCard;
            }

            ClearCardDisplay();
        }
    }
}
