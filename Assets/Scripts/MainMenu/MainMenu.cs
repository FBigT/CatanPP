using Assets.Scripts.Utils;
using Assets.Scripts.GameMode.Trading.Models;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button btnStartGame;
    public Button btnCancleStartGame;
    public Button btnProfile;
    public Button btnQuit;
    public Button btnLogout;

    public TMP_Text lblCode;
    public TMP_Text lblLoading;
    public TMP_Text lblStartError;

    public GameObject startPanel;
    public GameObject mainPanel;

    SessionManager _sessions;

    void Awake()
    {
        _sessions = gameObject.AddComponent<SessionManager>();
        gameObject.AddComponent<UserManager>();

        if (LocalStorageService.GetString("guest-code") != null)
            btnProfile.gameObject.SetActive(false);

        btnStartGame.onClick.AddListener(OnStartCampaignClicked);
        btnCancleStartGame.onClick.AddListener(
            () => _sessions.CloseSession(SessionClosed, SetError)
        );
        btnQuit.onClick.AddListener(Application.Quit);
        btnLogout.onClick.AddListener(() =>
        {
            LocalStorageService.ClearAll();
            SceneManager.LoadScene("Login");
        });
    }

    void OnStartCampaignClicked()
    {
        btnStartGame.interactable = false;
        lblLoading.gameObject.SetActive(true);
        _sessions.CreateSession(
            4,
            SessionCreated,
            _ => LoadOfflineGame()
        );
    }

    void SessionCreated(SessionCodeDto dto)
    {
        btnCancleStartGame.interactable = true;
        lblLoading.gameObject.SetActive(false);
        lblCode.text = dto.code;
        lblCode.gameObject.SetActive(true);

        // store the sessionId
        LocalStorageService.SetVariable("session-id", dto.sessionId.ToString());

        // go to the game scene
        SceneManager.LoadScene("GameModeCampaign");
    }

    void LoadOfflineGame()
    {
        lblStartError.text = "Starting offline campaign …";
        SceneManager.LoadScene("GameModeCampaign");
    }

    void SessionClosed()
    {
        lblCode.text = "";
        lblCode.gameObject.SetActive(false);
        lblLoading.gameObject.SetActive(true);

        mainPanel.SetActive(true);
        startPanel.SetActive(false);
    }

    void SetError(string msg) => lblStartError.text = msg;
}
