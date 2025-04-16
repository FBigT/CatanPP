using Assets.Scripts;
using Assets.Scripts.Utils;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button btnStartGame;
    public TMP_Text lblCode;
    public TMP_Text lblLoading;
    public TMP_Text lblStartError;
    public Button btnCancleStartGame;
    public Button btnProfile;
    public Button btnQuit;
    public Button btnLogout;
    public GameObject startPanel;
    public GameObject mainPanel;

    void Awake()
    {
        UserManager userManager = this.AddComponent<UserManager>();
        SessionManager sessionService = this.AddComponent<SessionManager>();

        if (LocalStorageService.GetString("guest-code") != null) { 
            btnProfile.gameObject.SetActive(false);
        }

        btnStartGame.onClick.AddListener(() => {
            sessionService.CreateSession(4, SessionCreated, SetError);
        });
        btnCancleStartGame.onClick.AddListener(() => {
            sessionService.CloseSession(SessionClosed, SetError);
        });
        btnQuit.onClick.AddListener(() =>
        {
            Application.Quit();
        });
        btnLogout.onClick.AddListener(() => {
            LocalStorageService.Clear();
            SceneManager.LoadScene("Login");
        });
    }

    private void SessionCreated(SessionCodeDto result) { 
        btnCancleStartGame.interactable = true;
        lblCode.text = result.code;
        lblCode.gameObject.SetActive(true);
        lblLoading.gameObject.SetActive(false);
        LocalStorageService.SetVariable("session-id", result.id);
    }

    private void SessionClosed() {
        lblCode.text = "";
        lblCode.gameObject.SetActive(false);
        lblLoading.gameObject.SetActive(true);
        mainPanel.SetActive(true);
        startPanel.SetActive(false);
    }

    private void SetError(string error) { 
        lblStartError.text = error;
    }
}
