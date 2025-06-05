using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VisualScripting;
using Assets.Scripts.User;
using System.Collections;

public class Register : MonoBehaviour
{
    EventSystem _eventSystem;
    UserManager userManager;

    public TMP_InputField ifEmail;
    public TMP_InputField ifUsername;
    public TMP_InputField ifPassword;
    public Selectable firstInput;
    public GameObject loginPanel;
    public GameObject registerPanel;
    public Button btnLogin;
    public Button btnRegister;
    public TMP_Text errorMessage;

    [Header("Slide Animation Settings")]
    public RectTransform registerPanelRect;
    public RectTransform loginPanelRect;
    public Vector2 slideStartPosition = new Vector2(1920f, 0f);
    public Vector2 slideEndPosition = new Vector2(0f, 0f);
    public float slideDuration = 0.25f;

    private Coroutine slideCoroutine;

    void Awake()
    {
        userManager = this.AddComponent<UserManager>();
        registerPanel.SetActive(false);
        firstInput.Select();
        _eventSystem = EventSystem.current;
        btnRegister.onClick.AddListener(() =>
            userManager.CreateUser(
                new RegisterForm(ifUsername.text, ifEmail.text, ifPassword.text),
                BackToLoginPanel,
                DisplayError
            ));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKeyDown(KeyCode.LeftShift))
        {
            Selectable previous = _eventSystem.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp();
            previous?.Select();
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            Selectable next = _eventSystem.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
            next?.Select();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            btnRegister.onClick.Invoke();
        }
    }

    private void BackToLoginPanel()
    {
        SlideOutRegisterPanel(); // instead of direct SetActive
    }

    private void DisplayError(string error)
    {
        errorMessage.text = error;
    }

    // --- Slide Methods ---

    public void SlideInRegisterPanel()
    {
        registerPanel.SetActive(true);
        registerPanelRect.anchoredPosition = slideStartPosition;

        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlidePanel(registerPanelRect, slideStartPosition, slideEndPosition, slideDuration));

        // Slide out login panel
        if (loginPanelRect != null && loginPanel.activeSelf)
        {
            if (slideCoroutine != null) StopCoroutine(slideCoroutine);
            slideCoroutine = StartCoroutine(SlidePanelAndDeactivate(loginPanelRect, slideEndPosition, slideStartPosition, slideDuration, loginPanel));
        }
    }

    public void SlideOutRegisterPanel()
    {
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlidePanelAndDeactivate(registerPanelRect, slideEndPosition, slideStartPosition, slideDuration, registerPanel));

        // Reactivate and slide in login panel
        loginPanel.SetActive(true);
        loginPanelRect.anchoredPosition = slideStartPosition;
        slideCoroutine = StartCoroutine(SlidePanel(loginPanelRect, slideStartPosition, slideEndPosition, slideDuration));
    }

    private IEnumerator SlidePanel(RectTransform panel, Vector2 from, Vector2 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            panel.anchoredPosition = Vector2.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        panel.anchoredPosition = to;
    }

    private IEnumerator SlidePanelAndDeactivate(RectTransform panel, Vector2 from, Vector2 to, float duration, GameObject targetToDisable)
    {
        yield return SlidePanel(panel, from, to, duration);
        targetToDisable.SetActive(false);
    }
}
