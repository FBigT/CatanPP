// Assets/Scripts/GameMode/Trading/TradeSentUI.cs
using UnityEngine;
using TMPro;

public class TradeSentUI : MonoBehaviour
{
    public TMP_Text messageText;

    public void Initialize(string message)
    {
        messageText.text = message;
        Destroy(gameObject, 2f);
    }
}
