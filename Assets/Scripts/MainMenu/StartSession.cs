using Assets.Scripts;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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

        btnCancleStartGame.onClick.AddListener(() => {
            sessionService.CloseSession(SessionClosed, SetError);
        });
    }

    // Update is called once per frame
    void Update()
    {
        WebSocketService.DispatchMessageQueue();
        if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(tfMessage.text))
        {
            WebSocketService.SendMessage(tfMessage.text);
            tfMessage.text = "";
        }
    }

    public void CreateSession() {
        mainPanel.SetActive(false);
        gameObject.SetActive(true);
        sessionService.CreateSession(4, SessionCreated, SetError);
    }

    private void SessionCreated(SessionCodeDto result)
    {
        WebSocketService.ConnectToChat(result.code, OnMessage);
        btnCancleStartGame.interactable = true;
        lblCode.text = result.code;
        lblCode.gameObject.SetActive(true);
        lblLoading.gameObject.SetActive(false);
        LocalStorageService.SetVariable("session-id", result.id);
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
        //Debug.Log(chatMessage);
    }
}
