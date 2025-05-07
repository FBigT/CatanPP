using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Assets.Scripts.Utils;                     // for SessionManager, LocalStorageService
using Assets.Scripts.TradingReasources.Models;  // for SessionCodeDto

namespace Assets.Scripts.MainMenu
{
    /// <summary>
    /// Handles showing the create-session form and creating a new game session.
    /// </summary>
    public class CreateGameUI : MonoBehaviour
    {
        [Header("Buttons & Panels")]
        [Tooltip("Button that opens the 'create session' form")]
        public Button createGameButton;

        [Tooltip("Panel containing the number-of-players input and submit button")]
        public GameObject createGamePanel;

        [Header("Form Fields")]
        [Tooltip("Input field for number of players (>=2)")]
        public TMP_InputField numPlayersInput;

        [Tooltip("Button to submit and create the session")]
        public Button submitButton;

        [Tooltip("Text to display errors (invalid input or server errors)")]
        public TMP_Text errorText;

        private SessionManager sessionManager;

        void Awake()
        {
            // attach the SessionManager for API calls
            sessionManager = gameObject.AddComponent<SessionManager>();

            // hide the create-session panel at start
            if (createGamePanel != null)
                createGamePanel.SetActive(false);

            // wire up the "Create Game" button
            createGameButton.onClick.AddListener(() =>
            {
                errorText.text = string.Empty;
                if (createGamePanel != null)
                    createGamePanel.SetActive(true);
            });

            // wire up the Submit button inside the panel
            submitButton.onClick.AddListener(OnSubmit);
        }

        private void OnSubmit()
        {
            errorText.text = string.Empty;

            // parse and validate the number of players
            if (!int.TryParse(numPlayersInput.text.Trim(), out int n) || n < 2)
            {
                errorText.text = "Please enter a valid number of players (>=2).";
                return;
            }

            // prevent double submissions
            submitButton.interactable = false;

            // call backend to create session
            sessionManager.CreateSession(
                n,
                OnSessionCreated,
                OnSessionError
            );
        }

        private void OnSessionCreated(SessionCodeDto dto)
        {
            // store session info for later screens
            LocalStorageService.SetVariable("session-id", dto.id.ToString());
            LocalStorageService.SetVariable("session-code", dto.code);

            // load campaign scene
            SceneManager.LoadScene("GameModeCampaign");
        }

        private void OnSessionError(string err)
        {
            // display error and re-enable submit
            errorText.text = err;
            submitButton.interactable = true;
        }
    }
}
