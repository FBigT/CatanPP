using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.DevCards.UI;

public class DevCardToggle : MonoBehaviour
{
    [Header("References")]
    public Button toggleButton;
    public DevCardPanel devCardPanel;

    private bool isPanelOpen = false;

    private void Start()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(TogglePanel);
        }

        // Start with panel closed
        if (devCardPanel != null)
        {
            devCardPanel.HidePanel();
            isPanelOpen = false;
            UpdateButtonText();
        }
    }

    private void TogglePanel()
    {
        if (devCardPanel != null)
        {
            if (isPanelOpen)
            {
                devCardPanel.HidePanel();
                isPanelOpen = false;
            }
            else
            {
                devCardPanel.ShowPanel();
                isPanelOpen = true;
            }

            UpdateButtonText();
        }
    }

    private void UpdateButtonText()
    {
        if (toggleButton != null)
        {
            var buttonText = toggleButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = isPanelOpen ? "Hide Cards" : "Dev Cards";
            }
        }
    }
}
