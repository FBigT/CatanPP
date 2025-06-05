using Assets.Scripts.Utils;                     
using Assets.Scripts.GameMode.Trading.Models; 
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;             
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
            StartCoroutine(sessionService.JoinSession(code, OnJoinSuccess, OnJoinError));
        });
    }

    private void OnJoinError(string message)
    {
        errorMessage.text = message;
    }

    private void OnJoinSuccess(SessionCodeDto dto)
    {
        LocalStorageService.SetVariable("session-id", dto.id.ToString());
        LocalStorageService.SetVariable("session-code", dto.code);

        SceneManager.LoadScene("GameModeCampaign");
    }
}
