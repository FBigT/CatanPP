using Assets.Scripts.Utils;
using Assets.Scripts.TradingReasources.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.User;
using Unity.VisualScripting;

public class StartSession : MonoBehaviour
{
    SessionManager sessionService;

    public TMP_Text lblCode;
    public TMP_Text lblLoading;
    public TMP_Text lblStartError;
    public TMP_InputField tfMessage;
    public Button btnCancleStartGame;
    public GameObject mainPanel;

    void Awake()
    {
        sessionService = this.AddComponent<SessionManager>();
        btnCancleStartGame.onClick.AddListener(() =>
        {
            sessionService.CloseSession(SessionClosed, SetError);
        });
    }

    void Update()
    {
        WebSocketService.DispatchMessageQueue();

        if (Input.GetKeyDown(KeyCode.Return)
            && !string.IsNullOrEmpty(tfMessage.text))
        {
            WebSocketService.SendMessage(tfMessage.text);
            tfMessage.text = "";
        }
    }

    public void CreateSession()
    {
        mainPanel.SetActive(false);
        gameObject.SetActive(true);
        lblLoading.gameObject.SetActive(true);

        sessionService.CreateSession(4, SessionCreated, SetError);
    }

    private void SessionCreated(SessionCodeDto dto)
    {
        WebSocketService.ConnectToChat(dto.code, OnMessage);

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
    }

    private void SetError(string error)
    {
        lblStartError.text = error;
    }

    private void OnMessage(ChatMessage chatMessage)
    {
        // handle incoming chat
    }
}
