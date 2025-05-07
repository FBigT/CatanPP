using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Utils;  // for LocalStorageService

namespace Assets.Scripts.GameMode
{
    /// <summary>
    /// Handles showing and hiding the “Invite Code” panel,
    /// displaying the current session code, back and copy buttons.
    /// </summary>
    public class InviteCodeUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Button in your HUD that opens the invite‐code panel")]
        public Button inviteButton;

        [Tooltip("Panel GameObject that shows the session code + buttons")]
        public GameObject invitePanel;

        [Tooltip("TMP_Text inside the panel where we display the code")]
        public TMP_Text codeText;

        [Tooltip("Button inside the panel to close it")]
        public Button backButton;

        [Header("Copy Functionality")]
        [Tooltip("Button inside the panel that copies the code to clipboard")]
        public Button copyButton;

        [Tooltip("Sprite to use for the copy button icon")]
        public Sprite copyIcon;

        void Awake()
        {
            // hide panel initially
            if (invitePanel != null)
                invitePanel.SetActive(false);

            // set the copy button icon if provided
            if (copyButton != null && copyIcon != null)
            {
                var img = copyButton.GetComponent<Image>();
                if (img != null) img.sprite = copyIcon;
            }

            // open panel and show current code
            inviteButton.onClick.AddListener(() =>
            {
                string code = LocalStorageService.GetString("session-code");
                codeText.text = string.IsNullOrEmpty(code)
                    ? "No session code available"
                    : code;
                invitePanel.SetActive(true);
            });

            // back hides panel
            backButton.onClick.AddListener(() =>
            {
                invitePanel.SetActive(false);
            });

            // copy puts code into clipboard
            copyButton.onClick.AddListener(() =>
            {
                GUIUtility.systemCopyBuffer = codeText.text;
                Debug.Log($"Invite code '{codeText.text}' copied to clipboard.");
            });
        }
    }
}
