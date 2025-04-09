using Assets.Scripts.Utils;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button btnStartGame;
    public TMP_Text sessionCode;
    public Button btnJoinGame;
    public Button btnProfile;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        UserManager userManager = this.AddComponent<UserManager>();
        SessionService sessionService = this.AddComponent<SessionService>();
        
    }
}
