using Assets.Scripts.Utils;              // for SessionManager, LocalStorageService
using Assets.Scripts.TradingReasources.Models;  // for SessionCodeDto
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.User;
using Unity.VisualScripting;

public class JoinGame : MonoBehaviour
{
    public TMP_InputField sessionCode;
    public GameObject joinControls;
    public Button btnJoinGame;
    public TMP_Text errorMessage;
    public TMP_Text lblSuccess;

    SessionManager sessionService;

    void Awake()
    {
        sessionService = this.AddComponent<SessionManager>();

        btnJoinGame.onClick.AddListener(() =>
        {
            ShowErrorMessage("");
            if (string.IsNullOrEmpty(sessionCode.text))
            {
                ShowErrorMessage("Please enter a valid session code");
                return;
            }
            sessionService.JoinSession(sessionCode.text, ShowSessionCode, ShowErrorMessage);
        });
    }

    public void ShowErrorMessage(string message)
    {
        errorMessage.text = message;
    }

    public void ShowSessionCode(SessionCodeDto dto)
    {
        // connect your chat socket
        WebSocketService.ConnectToChat(dto.code, OnMessage);

        // hide controls, show success
        joinControls.SetActive(false);
        lblSuccess.SetText($"Successfully joined game {dto.code}");

        // store session for later
        LocalStorageService.SetVariable("session-id", dto.sessionId.ToString());

        Debug.Log($"Joined session: {dto.sessionId}");
    }

    private void OnMessage(ChatMessage chatMessage)
    {
        Debug.Log(chatMessage);
    }
}
