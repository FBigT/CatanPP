using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Utils;
using UnityEngine.EventSystems;
using TMPro;

public class DebugMenu : Singleton<DebugMenu>, IPointerDownHandler, IDragHandler
{
    [Header("UI Elements")]
    public GameObject debugPanel;
    public Button moveThiefButton;
    public Button addAllResourcesButton;
    public Button clearLogButton;
    public Button addOneResourceButton;
    public TMP_Text statusText;

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.F12;

    private RectTransform panelRectTransform;
    private Vector2 pointerOffset;

    protected override void Awake()
    {
        base.Awake();

        if (debugPanel != null)
            debugPanel.SetActive(false);

        moveThiefButton.onClick.AddListener(() =>
        {
            ThifeManager.Instance.EnableThiefPlacement();
        });

        addAllResourcesButton.onClick.AddListener(() =>
        {
            // Your resource logic here
            AppendLog("+1 to all resources");
        });

        addOneResourceButton.onClick.AddListener(() =>
        {
            // Your one resource logic here
            AppendLog("+1 to all resources");
        });

        clearLogButton.onClick.AddListener(() =>
        {
            statusText.text = "";
        });

        panelRectTransform = debugPanel.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            debugPanel.SetActive(!debugPanel.activeSelf);
        }
    }

    public void AppendLog(string log)
    {
        statusText.text += $"{log}\n";
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform, eventData.position, eventData.pressEventCamera, out pointerOffset);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRectTransform.parent as RectTransform, eventData.position, eventData.pressEventCamera, out localPointerPosition))
        {
            panelRectTransform.localPosition = localPointerPosition - pointerOffset;
        }
    }
}
