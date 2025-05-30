using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GameMode.Trading
{
    [RequireComponent(typeof(Button))]
    public class TradeLauncherButton : MonoBehaviour
    {
        [Header("Panels")]
        [Tooltip("Drag in your Main Trade Panel (the one you copied over)")]
        [SerializeField] private GameObject mainTradePanel;

        [Header("Behavior")]
        [Tooltip("If true, clicking the button will toggle the panel on/off. If false, it only opens.")]
        [SerializeField] private bool togglePanel = true;

        void Awake()
        {
            if (mainTradePanel != null)
                mainTradePanel.SetActive(false);

            GetComponent<Button>().onClick.AddListener(OpenTradePanel);
        }

        private void OpenTradePanel()
        {
            if (mainTradePanel != null)
            {
                if (togglePanel)
                {
                    mainTradePanel.SetActive(!mainTradePanel.activeSelf);
                }
                else
                {
                    mainTradePanel.SetActive(true);
                }
            }
            else
            {
                Debug.LogWarning($"[TradeLauncherButton] mainTradePanel not assigned on {gameObject.name}");
            }
        }
    }
}
