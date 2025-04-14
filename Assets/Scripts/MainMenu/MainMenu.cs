using Assets.Scripts.Utils;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button btnStartGame;
    public Button btnCancleSearchGame;
    public Button btnProfile;

    void Awake()
    {
        UserManager userManager = this.AddComponent<UserManager>();
        SessionManager sessionService = this.AddComponent<SessionManager>();

        if (LocalStorageService.GetString("guest-code") != null) { 
            btnProfile.gameObject.SetActive(false);
        }
    }
}
