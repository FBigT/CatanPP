using Assets.Scripts;
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

    SessionManager sessionService;

    void Awake()
    {
        sessionService = this.AddComponent<SessionManager>();

        btnJoinGame.onClick.AddListener(() =>
        {
            ShowErrorMessage("");
            if (string.IsNullOrEmpty(sessionCode.text)) {
                ShowErrorMessage("Please enter a valid session code");
                return;
            }
            TryJoinGame(sessionCode.text);
        });
    }

    private void TryJoinGame(string code) {
        if (sessionService == null) {
            ShowErrorMessage("Please enter a session code");
            return;
        }

        sessionService.JoinSession(code, ShowSessionCode, ShowErrorMessage);
    }

    public void ShowErrorMessage(string message) 
    {
        errorMessage.text = message;
    }

    public void ShowSessionCode(SessionCodeDto sessionCodeDto)
    {
        Debug.Log(sessionCodeDto.id);
        Debug.Log(sessionCodeDto.code);
    }
}
