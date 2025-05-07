using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Utils;  // for LocalStorageService

namespace Assets.Scripts.GameMode
{
    /// <summary>
    /// Handles showing and hiding the “Invite Code” panel,
    /// displaying the current session code, and a Back button.
    /// </summary>
    public class InviteCodeUI : MonoBehaviour
    {
        [Header("UI References")]

        [Tooltip("Button in your HUD that opens the invite‐code panel")]
        public Button inviteButton;

        [Tooltip("Panel GameObject that shows the session code + Back button")]
        public GameObject invitePanel;

        [Tooltip("TMP_Text inside the panel where we display the code")]
        public TMP_Text codeText;

        [Tooltip("Button inside the panel that closes it")]
        public Button backButton;

        void Awake()
        {
            // ensure panel is hidden at start
            if (invitePanel != null)
                invitePanel.SetActive(false);

            // wire up the invite button
            inviteButton.onClick.AddListener(() =>
            {
                // fetch the code from local storage (set earlier when joining/creating)
                string code = LocalStorageService.GetString("session-code");
                codeText.text = string.IsNullOrEmpty(code)
                    ? "No session code available"
                    : code;

                invitePanel.SetActive(true);
            });

            // wire up the back button to hide the panel
            backButton.onClick.AddListener(() =>
            {
                invitePanel.SetActive(false);
            });
        }
    }
}
