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
    public class TradeBackButton : MonoBehaviour
    {
        [Tooltip("Drag the MainTradePanel GameObject here")]
        [SerializeField] private GameObject mainTradePanel;

        void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() =>
            {
                if (mainTradePanel != null)
                    mainTradePanel.SetActive(false);
                else
                    Debug.LogWarning($"[TradeBackButtonUI] mainTradePanel not assigned on {gameObject.name}");
            });
        }
    }
}
