using UnityEngine;
using UnityEngine.UIElements;
using CatanGame.DevCards.Core;
using System;

namespace CatanGame.DevCards.UI
{
    public class DevCardItem : MonoBehaviour
    {
        [Header("Dev Card Data")]
        [SerializeField] private DevCardData cardData;

        // UI Elements - these will be found by class/name in your existing UXML
        private VisualElement cardRoot;
        private VisualElement cardIcon;
        private Label titleLabel;
        private Label descLabel;
        private Button playButton;

        // Events
        public event Action<DevCardData> OnCardPlayed;

        private UIDocument uiDocument;

        public DevCardData CardData => cardData;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("DevCardItem requires UIDocument component!");
                return;
            }
        }

        private void OnEnable()
        {
            if (uiDocument?.rootVisualElement != null)
            {
                InitializeUI();
            }
        }

        private void OnDisable()
        {
            if (playButton != null)
            {
                playButton.clicked -= OnPlayButtonClicked;
            }
        }

        private void InitializeUI()
        {
            var root = uiDocument.rootVisualElement;

            // Find elements using your existing CSS classes
            cardRoot = root.Q<VisualElement>(className: "CardRoot");
            cardIcon = root.Q<VisualElement>(className: "card-icon");
            titleLabel = root.Q<Label>(className: "Title");
            descLabel = root.Q<Label>(className: "Desc");
            playButton = root.Q<Button>("PlayButton");

            if (playButton != null)
            {
                playButton.clicked += OnPlayButtonClicked;
            }

            // Update display if we have card data
            if (cardData != null)
            {
                UpdateDisplay();
            }
        }

        public void SetCardData(DevCardData data)
        {
            cardData = data;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (cardData == null) return;

            // Update text
            if (titleLabel != null)
                titleLabel.text = cardData.title;

            if (descLabel != null)
                descLabel.text = cardData.description;

            // Update icon background
            if (cardIcon != null && cardData.icon != null)
            {
                cardIcon.style.backgroundImage = new StyleBackground(cardData.icon);
            }

            // Update button state
            if (playButton != null)
            {
                playButton.SetEnabled(cardData.CanPlay());
                playButton.style.opacity = cardData.CanPlay() ? 1f : 0.5f;
            }

            // Update visual state based on usability
            if (cardRoot != null)
            {
                cardRoot.style.opacity = cardData.used ? 0.6f : 1f;
            }
        }

        private void OnPlayButtonClicked()
        {
            if (cardData != null && cardData.CanPlay())
            {
                OnCardPlayed?.Invoke(cardData);
            }
        }

        // Public method to refresh display (called externally when card state changes)
        public void RefreshDisplay()
        {
            UpdateDisplay();
        }
    }
}
