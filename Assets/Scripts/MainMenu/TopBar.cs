using Assets.Scripts.Utils;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TopBar : MonoBehaviour
{
    EventSystem _eventSystem;

    public Button btnStart;
    public Button btnPlay;
    public Button btnPlayer;

    void Awake()
    {
        UserManager userManager = this.AddComponent<UserManager>();

        _eventSystem = EventSystem.current;
        //btnStart.onClick.AddListener(() => sessionService.CreateSession());
        //btnPlay.onClick.AddListener(() => sessionService.JoinSession());
    }
}
