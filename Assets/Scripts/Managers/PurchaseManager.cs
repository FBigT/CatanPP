// Assets/Scripts/Managers/PurchaseManager.cs
using System;
using UnityEngine;
using Assets.Scripts.Enums;     // PurchaseType
using Catan.GameMode;          // Costs.Get(...)

namespace Catan.Managers
{
    [DefaultExecutionOrder(-100)]
    public sealed class PurchaseManager : MonoBehaviour
    {
        /* ── singleton ─────────────────────────────────────────── */
        public static PurchaseManager Instance { get; private set; }

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
        /// Sets which structure you want to build next—but DevCard is handled
        /// separately in the DevCardPanelController, so we ignore it here.
        /// </summary>
        public bool SetPurchase(PurchaseType type, int[] playerStock)
        {
            // 1) Block DevCard here so it doesn't interfere with your panel logic:
            if (type == PurchaseType.DevCard)
            {
                Debug.LogWarning("Dev Card purchases are handled via the DevCard panel.");
                return false;
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
