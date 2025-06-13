using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Utils;
using UnityEngine.EventSystems;
using TMPro;
using Assets.Scripts.Dtos.GameMoveResponses;
using Assets.Scripts.Dtos;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DebugMenu : Singleton<DebugMenu>, IPointerDownHandler, IDragHandler
{
    [Header("UI Elements")]
    public GameObject debugPanel;
    public Button moveThiefButton;
    public Button addAllResourcesButton;
    public Button clearLogButton;
    public Button addOneResourceButton;
    public Button btnStart;
    public TMP_Text statusText;
    public Button showVictorySceneButton;
    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.F12;

    private RectTransform panelRectTransform;
    private Vector2 pointerOffset;

    protected override void Awake()
    {
        base.Awake();

        if (debugPanel != null)
            debugPanel.SetActive(false);

        if (moveThiefButton != null)
        {
            moveThiefButton.onClick.AddListener(() =>
            {
                ThifeManager.Instance.EnableThiefPlacement();
            });
        }

        if (addAllResourcesButton != null)
        {
            addAllResourcesButton.onClick.AddListener(() =>
            {
                // Your resource logic here
                AppendLog("+1 to all resources");
            });
        }

        if (btnStart != null)
        {
            btnStart.onClick.AddListener(() =>
            {
                WebSocketService.SendStartGame();
            });
        }

        if (addOneResourceButton != null)
        {
            addOneResourceButton.onClick.AddListener(() =>
            {
                // Your one resource logic here
                AppendLog("+1 to all resources");
            });
        }

        if (showVictorySceneButton != null)
            showVictorySceneButton.onClick.AddListener(ShowVictorySceneManually);

        if (clearLogButton != null)
        {
            clearLogButton.onClick.AddListener(() =>
            {
                statusText.text = "";
            });
        }

        if(panelRectTransform != null)
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
    public void ShowVictorySceneManually()
    {
        // Create dummy data for testing
        var testVictory = new VictoryDto
        {
            players = new List<PlayerScoreDto>
        {
            new PlayerScoreDto { username = "Alice", score = 10 },
            new PlayerScoreDto { username = "Bob", score = 8 },
            new PlayerScoreDto { username = "Charlie", score = 6 }
        }
        };

        VictoryDataHolder.VictoryData = testVictory;
        SceneManager.LoadScene("VictoryScene"); // Use your actual scene name
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
