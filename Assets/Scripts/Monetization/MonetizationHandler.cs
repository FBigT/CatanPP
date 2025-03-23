using UnityEngine;
using UnityEngine.UI;

public class MonetizationHandler : MonoBehaviour
{
    public Button buyBattlePassButton;
    public Button buySkin1Button;
    public Button buySkin2Button;
    public Button buySkin3Button;

    private void Start()
    {
        buyBattlePassButton.onClick.AddListener(() => BuyItem("battle_pass"));
        buySkin1Button.onClick.AddListener(() => BuyItem("skin_1"));
        buySkin2Button.onClick.AddListener(() => BuyItem("skin_2"));
        buySkin3Button.onClick.AddListener(() => BuyItem("skin_3"));
    }

    private void BuyItem(string itemId)
    {
        Debug.Log($"🛒 Attempting to purchase {itemId}...");
        // ✅ Next step: Call PayPal API
    }
}
