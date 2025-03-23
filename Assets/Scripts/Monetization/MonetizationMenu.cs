using UnityEngine;
using UnityEngine.UI;

public class MonetizationMenu : MonoBehaviour
{
    public GameObject monetizationPanel; // Assign in Unity Inspector
    public Button storeButton; // Assign in Unity Inspector

    private void Start()
    {
        storeButton.onClick.AddListener(ToggleMonetizationMenu);
        monetizationPanel.SetActive(false); // Hide the menu at start
    }

    public void ToggleMonetizationMenu()
    {
        monetizationPanel.SetActive(!monetizationPanel.activeSelf);
    }
}
