using Assets.Scripts.Utils;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class JoinGame : MonoBehaviour
{
    public TMP_InputField sessionCode;
    public Button btnJoinGame;
    public TMP_Text errorMessage;

    SessionService sessionService;

    void Awake()
    {
        sessionService = GetComponent<SessionService>();

        btnJoinGame.onClick.AddListener(() => TryJoinGame(sessionCode.text));
    }

    private void TryJoinGame(string code) {
        if (sessionService == null) {
            ShowErrorMessage("Please enter a session code");
            return;
        }

        sessionService.JoinSession(code, ShowErrorMessage, ShowErrorMessage);
    }

    public void ShowErrorMessage(string message) {
        errorMessage.text = message;
    }
}
