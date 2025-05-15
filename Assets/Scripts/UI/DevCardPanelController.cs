using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Assets.Scripts.Models;    // DevCardDto, DevCardType
using Assets.Scripts.Utils;     // RequestService, Methods, LocalStorageService
using Catan.UI;

public class DevCardPanelController : MonoBehaviour
{
    [Header("UI Assets")]
    public VisualTreeAsset cardTemplate;     // assign DevCardItem.uxml in inspector

    private ScrollView _devScroll;
    private Button _closeBtn;
    private DevCardService _service;
    private long _playerId;

    void Awake()
    {
        // Add the service component at runtime
        _service = gameObject.AddComponent<DevCardService>();

        // Grab UI elements from the UIDocument
        var root = GetComponent<UIDocument>().rootVisualElement;
        _devScroll = root.Q<ScrollView>("DevCardScroll");
        _closeBtn = root.Q<Button>("CloseDevCardPanel");

        // first, log so we know the callback is firing:
        _closeBtn.clicked += () => Debug.Log("Close button clicked");

        // NOTE: if that still doesn’t show up, then click events really aren’t propagating.
        _closeBtn.clicked += () => gameObject.SetActive(false);

        // Alternately (more robust), use RegisterCallback:
        _closeBtn.RegisterCallback<ClickEvent>(evt => {
            Debug.Log("Close ClickEvent fired");
            gameObject.SetActive(false);
        });


        // Read player-id from PlayerPrefs (LocalStorageService)
        var pidString = LocalStorageService.GetString("player-id");
        long.TryParse(pidString, out _playerId);

        // Start hidden
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Opens the panel and refreshes the list from the backend.
    /// </summary>
    public void Open()
    {
        gameObject.SetActive(true);
        RefreshList();
    }

    /// <summary>
    /// Buys a new DevCard, then refreshes the list.
    /// </summary>
    public void BuyAndRefresh()
    {
        StartCoroutine(_service.Buy(
            card => {
                // Deduct resources in top bar
                TopBarUI.Instance.RefreshResources();
                // Refresh our list to show the newly acquired card
                RefreshList();
            },
            err => Debug.LogError("DevCard buy failed: " + err)
        ));
    }

    /// <summary>
    /// Clears and repopulates the scroll list with current cards.
    /// </summary>
    private void RefreshList()
    {
        _devScroll.Clear();
        StartCoroutine(_service.List(
            _playerId,
            cards => {
                foreach (var dto in cards)
                    AddCardItem(dto);
            },
            err => Debug.LogError("DevCard list failed: " + err)
        ));
    }

    /// <summary>
    /// Instantiates one card entry and populates its data.
    /// </summary>
    private void AddCardItem(DevCardDto dto)
    {
        var ve = cardTemplate.Instantiate();
        // Title
        ve.Q<Label>("Title").text = dto.type.ToString().Replace("_", " ");
        // Description
        ve.Q<Label>("Desc").text = dto.type switch
        {
            DevCardType.KNIGHT => "Move robber + largest army count",
            DevCardType.VICTORY_POINT => "Hidden 1 VP",
            DevCardType.ROAD_BUILDING => "Place 2 free roads",
            DevCardType.YEAR_OF_PLENTY => "Take any 2 resources",
            _ => ""
        };
        // Icon (set backgroundImage on our VisualElement)
        var iconVE = ve.Q<VisualElement>("Icon");
        var sprite = Resources.Load<Sprite>($"DevCards/{dto.type}");
        if (sprite != null)
            iconVE.style.backgroundImage = new StyleBackground(sprite.texture);

        // Play/Used/Locked button
        var btn = ve.Q<Button>("PlayButton");
        if (!dto.playable)
        {
            btn.text = "Locked";
            btn.SetEnabled(false);
        }
        else if (dto.used)
        {
            btn.text = "Used";
            btn.SetEnabled(false);
        }
        else
        {
            btn.text = "Play";
            btn.SetEnabled(true);
            btn.clicked += () => OnPlay(dto.id, btn);
        }

        _devScroll.Add(ve);
    }

    /// <summary>
    /// Calls the backend to “use” a card, then greys out the button.
    /// </summary>
    private void OnPlay(long cardId, Button btn)
    {
        StartCoroutine(_service.Use(
            cardId,
            usedDto => {
                btn.text = "Used";
                btn.SetEnabled(false);
            },
            err => Debug.LogError("DevCard use failed: " + err)
        ));
    }
}
