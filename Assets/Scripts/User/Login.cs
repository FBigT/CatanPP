using System.Collections;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    private UserManager userManager;
    private EventSystem _eventSystem;

    [Header("UI References")]
    public TMP_InputField ifUsername;
    public TMP_InputField ifPassword;
    public Selectable firstInput;
    public Button btnLogin;
    public Button btnRegister;
    public Button btnGuest;
    public TMP_Text errorMessage;

    [Header("Slide Animation Settings")]
    public RectTransform loginPanelRect;
    public Vector2 slideStartPosition = new Vector2(-1000, 0);
    public Vector2 slideEndPosition = new Vector2(0, 0);
    public float slideDuration = 0.5f;

    private Coroutine slideCoroutine;

    private bool isLoggingIn = false;

    void Start()
    {
        userManager = this.AddComponent<UserManager>();
        firstInput.Select();
        _eventSystem = EventSystem.current;

        btnLogin.onClick.AddListener(() => {
            SetErrorMessage("");

            if (IsUserLoggedIn())
            {
                Logout();
            }
            else
            {
                userManager.Login(
                    new LoginForm(ifUsername.text, ifPassword.text),
                    LoginSuccess,
                    SetErrorMessage
                );
            }
        });

        btnGuest.onClick.AddListener(() => {
            SetErrorMessage("");
            userManager.CreateGuest(GuestOnSuccess, SetErrorMessage);
            btnGuest.interactable = false;
        });

        LoginWithRefresh();
        LoginGuest();
        StartCoroutine(SelectFirstInputDelayed());

        UpdateLoginButtonLabel();
    }

    private bool IsUserLoggedIn()
    {
        string token = LocalStorageService.GetString("token");
        return !string.IsNullOrEmpty(token);
    }

    private void LoginSuccess(LoginResponse response)
    {
        isLoggingIn = false;
        LocalStorageService.SetVariable("token", response.tokenType + " " + response.token);
        LocalStorageService.SetVariable("refresh-token", response.refreshToken);
        LocalStorageService.SetVariable("username", response.username);

        UpdateLoginButtonLabel();
        //SceneManager.LoadScene("MainMenu");
    }

    private void LoginGuest()
    {
        if (isLoggingIn) return;

        var guestCode = LocalStorageService.GetString("guest-code");
        if (!string.IsNullOrEmpty(guestCode))
        {
            isLoggingIn = true;
            userManager.GuestLogin(guestCode, LoginSuccess, PrintError);
        }
    }

    private void LoginWithRefresh()
    {
        if (isLoggingIn) return;

        var refresh = LocalStorageService.GetString("refresh-token");
        if (!string.IsNullOrEmpty(refresh))
        {
            isLoggingIn = true;
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
        errorMessage.text = error;
    }

    private void PrintError(string error)
    {
        isLoggingIn = false;
        Debug.LogError(error);
        SetErrorMessage("Session expired. Please log in again.");

        btnLogin.interactable = true;
        btnRegister.interactable = true;
        btnGuest.interactable = true;

        UpdateLoginButtonLabel();
    }

    private void Logout()
    {
        LocalStorageService.ClearAll();
        UpdateLoginButtonLabel();
        SlideInLoginPanel();
    }

    private IEnumerator SelectFirstInputDelayed()
    {
        yield return null;
        firstInput.Select();
    }

    void Update()
    {
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

    public void SlideInLoginPanel()
    {
        if (loginPanelRect != null)
        {
            GameObject panelGO = loginPanelRect.gameObject;

            if (!panelGO.activeSelf)
                panelGO.SetActive(true);

            if (slideCoroutine != null)
                StopCoroutine(slideCoroutine);

            loginPanelRect.anchoredPosition = slideStartPosition;
            slideCoroutine = StartCoroutine(SlidePanel(loginPanelRect, slideStartPosition, slideEndPosition, slideDuration));
        }
    }

    public void SlideOutLoginPanel()
    {
        if (loginPanelRect != null)
        {
            if (slideCoroutine != null)
                StopCoroutine(slideCoroutine);

            slideCoroutine = StartCoroutine(SlidePanelAndDeactivate(loginPanelRect, slideEndPosition, slideStartPosition, slideDuration));
        }
    }

    private void UpdateLoginButtonLabel()
    {
        string token = LocalStorageService.GetString("token");
        btnLogin.GetComponentInChildren<TMP_Text>().text = string.IsNullOrEmpty(token) ? "Login" : "Logout";
    }

    private IEnumerator SlidePanelAndDeactivate(RectTransform panel, Vector2 from, Vector2 to, float duration)
    {
        yield return SlidePanel(panel, from, to, duration);
        panel.gameObject.SetActive(false);
    }

    private IEnumerator SlidePanel(RectTransform panel, Vector2 from, Vector2 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            panel.anchoredPosition = Vector2.Lerp(from, to, Mathf.SmoothStep(0, 1, t));
            elapsed += Time.deltaTime;
            yield return null;
        }
        panel.anchoredPosition = to;
    }
}
