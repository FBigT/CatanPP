using UnityEngine;
using UnityEngine.UIElements;
using CatanGame.DevCards.Core;
using System.Collections.Generic;
using System;

namespace CatanGame.DevCards.UI
{
    public class YearOfPlentyPanel : MonoBehaviour
    {
        [Header("UI Document")]
        [SerializeField] private UIDocument uiDocument;

        // UI Elements
        private VisualElement panelRoot;
        private DropdownField resourceDropdown1;
        private DropdownField resourceDropdown2;
        private Button confirmButton;
        private Button cancelButton;

        // Available resources
        private List<string> availableResources = new List<string>
        {
            "Wood", "Brick", "Crystal", "Ore", "Rice", "Sheep", "Silver", "Gold"
        };

        // Events
        public event Action<string, string> OnResourcesSelected;
        public event Action OnCancelled;

        private void Awake()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
        }

        private void Start()
        {
            InitializeUI();
            Hide();
        }

        private void InitializeUI()
        {
            if (uiDocument?.rootVisualElement == null) return;

            var root = uiDocument.rootVisualElement;

            panelRoot = root.Q<VisualElement>("YearOfPlentyPanel");
            resourceDropdown1 = root.Q<DropdownField>("ResourceDropdown1");
            resourceDropdown2 = root.Q<DropdownField>("ResourceDropdown2");
            confirmButton = root.Q<Button>("ConfirmButton");
            cancelButton = root.Q<Button>("CancelButton");

            // Setup dropdowns
            if (resourceDropdown1 != null)
            {
                resourceDropdown1.choices = availableResources;
                resourceDropdown1.value = availableResources[0];
            }

            if (resourceDropdown2 != null)
            {
                resourceDropdown2.choices = availableResources;
                resourceDropdown2.value = availableResources[0];
            }

            // Setup buttons
            if (confirmButton != null)
                confirmButton.clicked += OnConfirmClicked;

            if (cancelButton != null)
                cancelButton.clicked += OnCancelClicked;
        }

        public void Show()
        {
            if (panelRoot != null)
            {
                panelRoot.style.display = DisplayStyle.Flex;

                // Reset dropdowns to first option
                if (resourceDropdown1 != null)
                    resourceDropdown1.value = availableResources[0];
                if (resourceDropdown2 != null)
                    resourceDropdown2.value = availableResources[0];
            }
        }

        public void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.style.display = DisplayStyle.None;
            }
        }

        private void OnConfirmClicked()
        {
            if (resourceDropdown1?.value != null && resourceDropdown2?.value != null)
            {
                OnResourcesSelected?.Invoke(resourceDropdown1.value, resourceDropdown2.value);
                Hide();
            }
        }

        private void OnCancelClicked()
        {
            OnCancelled?.Invoke();
            Hide();
        }

        private void OnDestroy()
        {
            if (confirmButton != null)
                confirmButton.clicked -= OnConfirmClicked;
            if (cancelButton != null)
                cancelButton.clicked -= OnCancelClicked;
        }
    }
}
