using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Enums;

public class PurchaseUIHighlighter : MonoBehaviour
{
    [System.Serializable]
    public class HighlightEntry
    {
        public PurchaseType type;
        public Image uiElement;
        public Animator animator;
        public string highlightTrigger = "Select";
        public string resetTrigger = "Deselect"; // Optional
    }

    [Header("Highlight Settings")]
    [SerializeField] private List<HighlightEntry> highlightEntries = new();

    private Dictionary<PurchaseType, HighlightEntry> entryLookup;
    private PurchaseType? currentlyHighlighted;

    void Awake()
    {
        entryLookup = new();
        foreach (var entry in highlightEntries)
        {
            if (entry.uiElement == null || entry.animator == null)
            {
                Debug.LogWarning($"HighlightEntry for {entry.type} is missing UI or Animator.");
                continue;
            }

            entryLookup[entry.type] = entry;
        }
    }

    public void Highlight(PurchaseType type)
    {
        if (currentlyHighlighted == type)
            return;

        ResetAll();

        if (entryLookup.TryGetValue(type, out var entry))
        {
            if (!string.IsNullOrEmpty(entry.highlightTrigger))
                entry.animator.SetTrigger(entry.highlightTrigger);

            currentlyHighlighted = type;
        }
    }

    public void ResetAll()
    {
        foreach (var entry in highlightEntries)
        {
            if (entry.animator != null && !string.IsNullOrEmpty(entry.resetTrigger))
                entry.animator.SetTrigger(entry.resetTrigger);
        }

        currentlyHighlighted = null;
    }
}
