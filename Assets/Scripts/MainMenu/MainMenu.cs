using Assets.Scripts.Utils;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button btnStartGame;
    public Button btnProfile;
    public Button btnQuit;
    public Button btnLogout;
    public GameObject startPanel;

    void Awake()
    {
        UserManager userManager = this.AddComponent<UserManager>();
        SessionManager sessionService = this.AddComponent<SessionManager>();
        StartSession startSessionScript = startPanel.GetComponentInChildren<StartSession>();

        if (LocalStorageService.GetString("guest-code") != null) { 
            btnProfile.gameObject.SetActive(false);
        }

        btnStartGame.onClick.AddListener(() => {
            startSessionScript.CreateSession();
        });
        
        btnQuit.onClick.AddListener(() => {
            Application.Quit();
        });

        btnLogout.onClick.AddListener(() => {
            LocalStorageService.Clear();
            SceneManager.LoadScene("Login");
        });
    }
}
