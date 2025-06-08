using Assets.Scripts.Utils;
using Assets.Scripts.GameMode.Trading.Models;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Assets.Scripts.User;

public class StartSession : MonoBehaviour
{
    public TMP_Text lblCode;
    public TMP_Text lblLoading;
    public TMP_Text lblStartError;
    public TMP_InputField tfMessage;
    public Button btnCancleStartGame;
    public GameObject mainPanel;

    async void Awake()
    {
        btnCancleStartGame.onClick.AddListener(() =>
        {
            SessionManager.Instance.CloseSession(SessionClosed, SetError);
        });

        // Ensure we have a valid token before any WebSocket traffic
        // (you can remove if EnsureAuthToken already covers this)
        string token = LocalStorageService.GetString("token") ?? "";
        Debug.Log($"[StartSession] auth token = {token}");
    }

    void Update()
    {
        // Dispatch any pending WebSocket messages (still synchronous)
        WebSocketService.DispatchMessageQueue();

        if (Input.GetKeyDown(KeyCode.Return)
            && !string.IsNullOrEmpty(tfMessage.text))
        {
            // fire-and-forget send
            _ = WebSocketService.SendMessage(tfMessage.text);
            tfMessage.text = "";
        }
    }

    public void CreateSession()
    {
        mainPanel.SetActive(false);
        gameObject.SetActive(true);
        lblLoading.gameObject.SetActive(true);

        SessionManager.Instance.CreateSession(4, SessionCreated, SetError);
    }

    // now async so we can await ConnectToChat
    private async void SessionCreated(SessionCodeDto dto)
    {
        Debug.Log("Session created");
        WebSocketService.OnChatMessageReceived += OnMessage;
        // connect and wait
        await WebSocketService.ConnectToChat(dto.code);

        btnCancleStartGame.interactable = true;
        lblLoading.gameObject.SetActive(false);
        lblCode.text = dto.code;
        lblCode.gameObject.SetActive(true);

        // store the sessionId
        LocalStorageService.SetVariable("session-id", dto.sessionId.ToString());
    }

    private void SessionClosed()
    {
        lblCode.text = "";
        lblCode.gameObject.SetActive(false);
        lblLoading.gameObject.SetActive(true);
        mainPanel.SetActive(true);
        gameObject.SetActive(false);
        WebSocketService.OnChatMessageReceived -= OnMessage;
    }

    private void SetError(string error)
    {
        lblStartError.text = error;
    }

    private void OnMessage(ChatMessage chatMessage)
    {
        Debug.Log(chatMessage);
    }
}
