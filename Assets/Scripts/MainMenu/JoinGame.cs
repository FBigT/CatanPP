using Assets.Scripts.Utils;                     // for SessionManager, LocalStorageService
using Assets.Scripts.TradingReasources.Models;  // for SessionCodeDto
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;              // for SceneManager
using UnityEngine.UI;
using Unity.VisualScripting;

public class JoinGame : MonoBehaviour
{
    [Header("Join UI")]
    public TMP_InputField sessionCode;
    public Button btnJoinGame;
    public TMP_Text errorMessage;

    private SessionManager sessionService;

    void Awake()
    {
        sessionService = this.AddComponent<SessionManager>();

        btnJoinGame.onClick.AddListener(() =>
        {
            errorMessage.text = "";
            var code = sessionCode.text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                errorMessage.text = "Please enter a valid session code.";
                return;
            }
            sessionService.JoinSession(code, OnJoinSuccess, OnJoinError);
        });
    }

    private void OnJoinError(string message)
    {
        // if the server returns 400, this will be called
        errorMessage.text = message;
    }

    private void OnJoinSuccess(SessionCodeDto dto)
    {
        // persist for later (trade screen, etc.)
        LocalStorageService.SetVariable("session-id", dto.id.ToString());
        LocalStorageService.SetVariable("session-code", dto.code);

        // now load the campaign scene immediately
        SceneManager.LoadScene("GameModeCampaign");
    }
}
