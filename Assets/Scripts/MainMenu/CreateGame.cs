/*using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Utils;
using Assets.Scripts.TradingReasources.Models;
using TMPro;

namespace Assets.Scripts.MainMenu
{
    [RequireComponent(typeof(Button))]
    public class CreateGameUGUI : MonoBehaviour
    {
        [Header("Create-Session Panel")]
        [Tooltip("Panel containing the Create-Session UI (number input + submit)")]
        [SerializeField] private GameObject createGamePanel;

        void Awake()
        {
            var btn = GetComponent<Button>();
            btn.onClick.AddListener(OnCreateGameClicked);

            // ensure panel is hidden initially
            if (createGamePanel != null)
                createGamePanel.SetActive(false);
        }

        private void OnCreateGameClicked()
        {
            if (createGamePanel != null)
            {
                createGamePanel.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"[CreateGameUGUI] createGamePanel not assigned on {gameObject.name}");
            }
        }
    }
}
*/