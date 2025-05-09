using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                mainTradePanel.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"[TradeLauncherUGUI] mainTradePanel not assigned on {gameObject.name}");
            }
        }
    }
}
