using UnityEngine;
using UnityEngine.UIElements;

namespace Catan.GameMode.UI
{
    /// <summary>
    /// Looks for a <see cref="Button"/> called *EndTurnButton* inside the
    /// UIDocument on the same GameObject and calls <see cref="CampaignGameMode.EndTurn"/>.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class EndTurnUITK : MonoBehaviour
    {
        [SerializeField] string buttonName = "EndTurnButton";

        void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            var btn = root.Q<Button>(buttonName);

            if (btn == null)
            {
                Debug.LogError($"EndTurnUITK -- Button “{buttonName}” not found.");
                return;
            }

            btn.clicked += OnClicked;
        }

        void OnClicked()
        {
            var gm = CampaignGameMode.Instance;
            if (gm == null)               // game-logic not booted yet
            {
                Debug.LogWarning("EndTurnUITK -- GameMode not ready.");
                return;
            }

            const int localSeat = 0;      // seat 0 is the human player
            if (!gm.IsPlayerTurn(localSeat))
            {
                Debug.Log("EndTurnUITK -- Not your turn.");
                return;
            }

            gm.EndTurn();                 // ✅ advance to the next player
        }
    }
}
