using System;
using UnityEngine;
using Assets.Scripts.Enums;     // PurchaseType
using Catan.GameMode;          // Costs.Get(...)
using Assets.Scripts.DevCards.UI; // DevCardPanel

namespace Catan.Managers
{
    [DefaultExecutionOrder(-100)]
    public sealed class PurchaseManager : MonoBehaviour
    {
        /* ── singleton ─────────────────────────────────────────── */
        public static PurchaseManager Instance { get; private set; }

        [Header("Dev Card Panel Reference")]
        public DevCardPanel devCardPanel; // Add this field in inspector

        private bool isPanelOpen = false; // Track panel state

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /* ── chosen purchase ───────────────────────────────────── */
        public PurchaseType Selected { get; private set; } = PurchaseType.None;

        /* ---------- legacy alias (for older scripts) ------------ */
        public PurchaseType SelectedPurchase => Selected;

        public event Action<PurchaseType> OnPurchaseChanged;

        /* ── API called by UI scripts ───────────────────────────── */

        /// <summary>
        /// Sets which structure you want to build next—but DevCard toggles the panel.
        /// </summary>
        public bool SetPurchase(PurchaseType type, int[] playerStock)
        {
            // Handle dev cards as panel toggle instead of buying
            if (type == PurchaseType.DevCard)
            {
                ToggleDevCardPanel();
                return true;
            }

            // 2) Affordability check for the other types:
            if (playerStock != null && !CanAfford(type, playerStock))
                return false;  // can't afford → ignore

            // 3) Commit the selection and notify listeners:
            Selected = type;
            OnPurchaseChanged?.Invoke(type);
            return true;
        }

        /// <summary>
        /// Keep the old overload working (it now respects the DevCard check above).
        /// </summary>
        public bool SetPurchase(PurchaseType type) => SetPurchase(type, null);

        /// <summary>Clears the current selection (modern name).</summary>
        public void Clear()
        {
            Selected = PurchaseType.None;
            OnPurchaseChanged?.Invoke(Selected);
        }

        /* ---------- legacy alias (for older scripts) ------------ */
        public void ClearPurchase() => Clear();

        /* ── dev card panel toggle ─────────────────────────────── */
        private void ToggleDevCardPanel()
        {
            if (devCardPanel != null)
            {
                if (isPanelOpen)
                {
                    devCardPanel.HidePanel();
                    isPanelOpen = false;
                    Debug.Log("Dev card panel hidden");
                }
                else
                {
                    devCardPanel.ShowPanel();
                    isPanelOpen = true;
                    Debug.Log("Dev card panel shown");
                }
            }
            else
            {
                Debug.LogError("DevCardPanel reference not assigned in PurchaseManager! Please assign it in the inspector.");
            }
        }

        /* ── helpers ────────────────────────────────────────────── */
        public bool CanAfford(PurchaseType type, int[] have)
        {
            if (have == null) return true;            // no stock info → skip check
            int[] need = Costs.Get(type);
            for (int i = 0; i < need.Length && i < have.Length; i++)
                if (have[i] < need[i]) return false;
            return true;
        }
    }
}
