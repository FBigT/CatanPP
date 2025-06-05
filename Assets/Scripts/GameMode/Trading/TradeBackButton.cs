using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GameMode.Trading
{
    [RequireComponent(typeof(Button))]
    public class TradeBackButton : MonoBehaviour
    {
        [Tooltip("Drag the GameObject that has TradeScreenManager on it")]
        [SerializeField]
        private TradeScreenManager tradeScreenManager;

        [Tooltip("Drag the MainTradePanel GameObject here")]
        [SerializeField]
        private GameObject mainTradePanel;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnBackClicked);
        }

        private void OnBackClicked()
        {
            // 1) Zero‐out every resource button
            if (tradeScreenManager != null)
            {
                tradeScreenManager.ResetAllResourceSelections();
            }
            else
            {
                Debug.LogWarning($"[TradeBackButton] Missing TradeScreenManager reference on {gameObject.name}");
            }

            // 2) Hide the MainTradePanel itself
            if (mainTradePanel != null)
            {
                mainTradePanel.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"[TradeBackButton] mainTradePanel not assigned on {gameObject.name}");
            }
        }
    }
}
