using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VisualScripting;

public class Register : MonoBehaviour
{
    EventSystem _eventSystem;


    public TMP_InputField ifEmail;
    public TMP_InputField ifUsername;
    public TMP_InputField ifPassword;
    public Selectable firstInput;
    public GameObject RegisterPanel;
    public Button btnLogin;
    public Button btnRegister;

    void Awake()
    {
        UserManager userManager = this.AddComponent<UserManager>();
        RegisterPanel.SetActive(false);
        firstInput.Select();
        _eventSystem = EventSystem.current;
        btnRegister.onClick.AddListener(() => userManager.CreateUser(ifUsername.text, ifEmail.text, ifPassword.text));
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
            if (next != null)
            {
                next.Select();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            btnRegister.onClick.Invoke();
        }
    }
}
