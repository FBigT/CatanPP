// Assets/Scripts/UI/Test/Login.cs
using System.Collections;
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
    UserManager userManager; // assign via Inspector
    EventSystem _eventSystem;

    public TMP_InputField ifUsername;
    public TMP_InputField ifPassword;
    public Selectable firstInput;
    public Button btnLogin;
    public Button btnRegister;
    public Button btnGuest;
    public TMP_Text errorMessage;

    void Start()
    {
        userManager = this.AddComponent<UserManager>();
        firstInput.Select();
        _eventSystem = EventSystem.current;

        btnLogin.onClick.AddListener(() => {
            SetErrorMessage("");
            userManager.Login(
                new LoginForm(ifUsername.text, ifPassword.text),
                LoginSuccess,
                SetErrorMessage
            );
        });

        btnGuest.onClick.AddListener(() => {
            SetErrorMessage("");
            userManager.CreateGuest(GuestOnSuccess, SetErrorMessage);
            btnGuest.interactable = false;
        });

        // Try refresh/guest auto?login if tokens are already in storage
        LoginWithRefresh();
        LoginGuest();
        StartCoroutine(SelectFirstInputDelayed());
    }

    private void LoginSuccess(LoginResponse response)
    {
        // store the JWT for subsequent API calls
        LocalStorageService.SetVariable("token", response.tokenType + " " + response.token);
        LocalStorageService.SetVariable("refresh-token", response.refreshToken);

        // store the username so TradeScreen can read it
        // adjust property name here if your LoginResponse uses "username" vs "userName"
        LocalStorageService.SetVariable("username", response.username);

        // now go to main menu
        SceneManager.LoadScene("MainMenu");
    }

    private void LoginGuest()
    {
        var guestCode = LocalStorageService.GetString("guest-code");
        if (!string.IsNullOrEmpty(guestCode))
        {
            userManager.GuestLogin(guestCode, LoginSuccess, PrintError);
        }
    }

    private void LoginWithRefresh()
    {
        var refresh = LocalStorageService.GetString("refresh-token");
        if (!string.IsNullOrEmpty(refresh))
        {
            userManager.RefreshToken(refresh, LoginSuccess, PrintError);
        }
    }

    private void GuestOnSuccess(GuestRegisterResponse guestResponse)
    {
        LocalStorageService.SetVariable("guest-code", guestResponse.guestKey);
        LoginGuest();
    }

    private void SetErrorMessage(string error)
    {
        if (error.Contains("401"))
        {
            errorMessage.text = "Bad credentials";
        }
        else
        {
            errorMessage.text = error;
        }
    }

    private void PrintError(string error)
    {
        Debug.LogError(error);
        SetErrorMessage("Session expired. Please log in again.");

        // Re-enable buttons
        btnLogin.interactable = true;
        btnRegister.interactable = true;
        btnGuest.interactable = true;

        Debug.Log("About to start coroutine to select input");

         // Ensure this is inside the Login MonoBehaviour
    }

    private IEnumerator SelectFirstInputDelayed()
    {
        yield return null; // Wait one frame
        Debug.Log("Now selecting input");
        firstInput.Select();
    }

    void Update()
    {
        // tab/shift-tab navigation & Enter = login
        if (Input.GetKeyDown(KeyCode.Tab) && _eventSystem.currentSelectedGameObject != null)
        {
            var sel = _eventSystem.currentSelectedGameObject.GetComponent<Selectable>();
            if (sel != null)
            {
                var next = Input.GetKey(KeyCode.LeftShift)
                    ? sel.FindSelectableOnUp()
                    : sel.FindSelectableOnDown();
                next?.Select();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            btnLogin.onClick.Invoke();
        }
    }
}
