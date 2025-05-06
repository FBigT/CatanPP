// Assets/Scripts/UI/TurnBannerUI.cs
using UnityEngine;
using UnityEngine.UIElements;
using Catan.GameMode;
using Assets.Scripts.Enums;      // PurchaseType
using Catan.Managers;           // PurchaseManager

[RequireComponent(typeof(UIDocument))]
public class TurnBannerUI : MonoBehaviour
{
    public static TurnBannerUI Instance { get; private set; }
    private Label _label;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        var doc = GetComponent<UIDocument>();
        _label = doc.rootVisualElement.Q<Label>("TurnLabel");
        if (_label == null) Debug.LogWarning("TurnLabel not found in UXML.");
    }

    /// <summary>Called when either setup or play turns change.</summary>
    public void ShowTurn(PlayerState p, GamePhase phase)
    {
        if (_label == null) return;

        if (phase == GamePhase.Setup)
        {
            var next = PurchaseManager.Instance.SelectedPurchase;
            _label.text = p.Seat == 0
                ? $"Setup: place {next}"
                : $"{p.DisplayName} is placing {next}…";
        }
        else // GamePhase.Play
        {
            _label.text = p.Seat == 0
                ? "Your turn"
                : $"{p.DisplayName} is thinking…";
        }
    }
}
