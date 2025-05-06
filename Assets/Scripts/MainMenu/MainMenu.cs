// Assets/Scripts/MainMenu/MainMenu.cs
using Assets.Scripts;
using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
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

    /* ─────────────────────────────────────────────────────────────── */
    SessionManager _sessions;

    void Awake()
    {
        // runtime helpers (adds components only once)
        _sessions = gameObject.AddComponent<SessionManager>();
        gameObject.AddComponent<UserManager>();

        if (LocalStorageService.GetString("guest-code") != null)
            btnProfile.gameObject.SetActive(false);

        /* ---------- button wiring ---------------------------------- */
        btnStartGame.onClick.AddListener(OnStartCampaignClicked);
        btnCancleStartGame.onClick.AddListener(
            () => _sessions.CloseSession(SessionClosed, SetError));

        btnQuit.onClick.AddListener(Application.Quit);
        btnLogout.onClick.AddListener(() =>
        {
            LocalStorageService.Clear();
            SceneManager.LoadScene("Login");
        });
    }

    /* ─────────────────────────────────────────────────────────────── */
    /*  1) “Start Campaign” → close any open game then create a new one */
    void OnStartCampaignClicked()
    {
        _sessions.CreateSession(
            4,
            SessionCreated,          // success → normal flow
            _ => LoadOfflineGame()   // any error → offline map
        );
    }

    /*  2) backend answers with code → show HUD, join, load scene       */
    void SessionCreated(SessionCodeDto result)
    {
        btnCancleStartGame.interactable = true;
        lblCode.text = result.code;
        lblCode.gameObject.SetActive(true);
        lblLoading.gameObject.SetActive(false);

        LocalStorageService.SetVariable("session-id", result.id);

        SceneManager.LoadScene("GameModeCampaign");                                    // fail
    }
    void LoadOfflineGame()
    {
        lblStartError.text = "Starting offline campaign …";
        SceneManager.LoadScene("GameModeCampaign");   // map is generated locally
    }
    /* ─────────────────────────────────────────────────────────────── */
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
