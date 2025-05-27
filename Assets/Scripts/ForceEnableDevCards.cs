using UnityEngine;
using UnityEngine.UI;

public class ForceEnableDevCards : MonoBehaviour
{
    [Header("Force Enable Dev Cards")]
    public Button devCardsButton;

    private void Start()
    {
        // Force enable immediately
        ForceEnable();

        // Keep forcing it every frame (for testing)
        InvokeRepeating(nameof(ForceEnable), 0.1f, 0.1f);
    }

    private void ForceEnable()
    {
        if (devCardsButton != null)
        {
            devCardsButton.interactable = true;
            Debug.Log("Dev Cards button forced enabled!");
        }
    }
}
