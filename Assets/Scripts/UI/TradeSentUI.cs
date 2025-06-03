using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeSentUI : MonoBehaviour
{
    public TMP_Text messageText;

    /// <summary>
    /// Called immediately after Instantiating the prefab.
    /// </summary>
    public void Initialize(string targetUser)
    {
        messageText.text = $"Trade offer sent to {targetUser}.";
        // Auto‐destroy after 2 seconds:
        Destroy(gameObject, 2f);
    }
}
