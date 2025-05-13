// Assets/Scripts/UI/Test/ResourceTestManager.cs
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Catan.UI;            // TopBarUI
using Catan.GameMode;      // CampaignGameMode

namespace Catan.UI.Test
{
    /// <summary>
    /// Hook this up to your “Add dummy resources” button.
    /// </summary>
    public class ResourceTestManager : MonoBehaviour, IPointerClickHandler
    {
        int _step = 0;

        public void OnPointerClick(PointerEventData eventData)
        {
            // step from 1→2→3… etc.
            _step++;
            int[] res = Enumerable.Repeat(_step, 8).ToArray();
            Debug.Log($"[Test] setting all resources to {_step}: {string.Join(",", res)}");

            // 1) overwrite the real game‐state
            var player = CampaignGameMode.Instance.CurrentPlayer;
            for (int i = 0; i < player.Resources.Length; i++)
                player.Resources[i] = res[i];

            // 2) update the HUD (this fires TopBarUI.OnResourcesChanged)
            TopBarUI.Instance.SendMessage("SetValues", res);

            // 3) simulate “it’s your play‐turn”
            CampaignGameMode.Instance.SimulateUiGate(
                /* isMyTurn:  */ true,
                /* inSetupPhase: */ false
            );
        }
    }
}
