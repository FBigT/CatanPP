using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VisualScripting;

public class Login : MonoBehaviour
{
    EventSystem _eventSystem;

    public TMP_InputField ifUsername;
    public TMP_InputField ifPassword;
    public Selectable firstInput;
    public Button btnLogin;
    public Button btnRegister;
    public Button btnGuest;

    void Awake()
    {
        UserManager userManager = this.AddComponent<UserManager>();
        firstInput.Select();
        _eventSystem = EventSystem.current;
        btnLogin.onClick.AddListener(() => userManager.Login(ifUsername.text, ifPassword.text));
        btnGuest.onClick.AddListener(() => {
            userManager.CreateGuest();
            btnGuest.interactable = false;
        });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKeyDown(KeyCode.LeftShift))
        {
            Selectable previous = _eventSystem.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp();
            if (previous != null)
            {
                previous.Select();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable next = _eventSystem.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
            if (next != null) { 
                next.Select();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            btnLogin.onClick.Invoke();
        }
    }
}
