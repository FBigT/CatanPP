using Assets.Scripts.User;
using Assets.Scripts.Utils;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    UserManager userManager;
    EventSystem _eventSystem;

    public TMP_InputField ifUsername;
    public TMP_InputField ifPassword;
    public Selectable firstInput;
    public Button btnLogin;
    public Button btnRegister;
    public Button btnGuest;
    public TMP_Text errorMessage;

    void Awake()
    {
        userManager = this.AddComponent<UserManager>();
        firstInput.Select();
        _eventSystem = EventSystem.current;
        btnLogin.onClick.AddListener(() => {
            SetErrorMessage("");
            userManager.Login(new LoginForm(ifUsername.text, ifPassword.text), LoginSuccess, SetErrorMessage);
        });
        btnGuest.onClick.AddListener(() => {
            SetErrorMessage("");
            userManager.CreateGuest(GuestOnSuccess, SetErrorMessage);
            btnGuest.interactable = false;
        });
        Debug.Log(LocalStorageService.GetString("refresh-token"));
        Debug.Log(LocalStorageService.GetString("token"));
        //PlayerPrefs.DeleteAll();
        LoginWithRefresh();
        LoginGuest();
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

    private void LoginSuccess(LoginResponse response) {
        LocalStorageService.SetVariable("token", response.tokenType + " " + response.token);
        LocalStorageService.SetVariable("refresh-token", response.refreshToken);
        SceneManager.LoadScene("MainMenu");
    }

    private void LoginGuest() {
        if (LocalStorageService.GetString("guest-code") != null)
        {
            userManager.GuestLogin(LocalStorageService.GetString("guest-code"), LoginSuccess, PrintError);
        }
    }

    private void LoginWithRefresh()
    {
        if (LocalStorageService.GetString("refresh-token") != null)
        {
            userManager.RefreshToken(LocalStorageService.GetString("refresh-token"), LoginSuccess, PrintError);
        }
    }

    private void GuestOnSuccess(GuestRegisterResponse guestResponse) {
        LocalStorageService.SetVariable("guest-code", guestResponse.guestKey);
        LoginGuest();
    }

    private void SetErrorMessage(string error){
        errorMessage.text = error;
    }

    private void PrintError(string error) { 
        Debug.Log(error);
    }
}
