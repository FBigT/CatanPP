using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace CatanGame.DevCards.UI
{
    public class RoadBuildingPanel : MonoBehaviour
    {
        [Header("UI Document")]
        [SerializeField] private UIDocument uiDocument;

        // UI Elements
        private VisualElement panelRoot;
        private Label instructionLabel;
        private Button cancelButton;

        // State
        private int roadsToPlace = 2;

        // Events
        public event Action<int> OnRoadPlaced;
        public event Action OnCancelled;
        public event Action OnCompleted;

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

            panelRoot = root.Q<VisualElement>("RoadBuildingPanel");
            instructionLabel = root.Q<Label>("InstructionLabel");
            cancelButton = root.Q<Button>("CancelButton");

            if (cancelButton != null)
                cancelButton.clicked += OnCancelClicked;
        }

        public void Show()
        {
            roadsToPlace = 2;
            UpdateInstruction();

            if (panelRoot != null)
            {
                panelRoot.style.display = DisplayStyle.Flex;
            }
        }

        public void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.style.display = DisplayStyle.None;
            }
        }

        public void OnRoadPlacedCallback()
        {
            roadsToPlace--;
            OnRoadPlaced?.Invoke(roadsToPlace);

            if (roadsToPlace <= 0)
            {
                OnCompleted?.Invoke();
                Hide();
            }
            else
            {
                UpdateInstruction();
            }
        }

        private void UpdateInstruction()
        {
            if (instructionLabel != null)
            {
                instructionLabel.text = $"Place {roadsToPlace} free road(s). Click on edges to place roads.";
            }
        }

        private void OnCancelClicked()
        {
            OnCancelled?.Invoke();
            Hide();
        }

        private void OnDestroy()
        {
            if (cancelButton != null)
                cancelButton.clicked -= OnCancelClicked;
        }
    }
}
