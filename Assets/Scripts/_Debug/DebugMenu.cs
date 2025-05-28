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
    public TMP_Text statusText;

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.F12;

    private RectTransform panelRectTransform;
    private Vector2 pointerOffset;

    private void Awake()
    {
        base.Awake();

        if (debugPanel != null)
            debugPanel.SetActive(false);

        moveThiefButton.onClick.AddListener(MoveThief);

        panelRectTransform = debugPanel.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            debugPanel.SetActive(!debugPanel.activeSelf);
        }
    }

    private void MoveThief()
    {
        if (ThifeManager.Instance != null)
        {
            statusText.text = "Thief moved!";
        }
        else
        {
            statusText.text = "ThifeManager instance not found.";
        }
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
