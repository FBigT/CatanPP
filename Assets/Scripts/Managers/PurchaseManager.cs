using System;
using UnityEngine;
using Assets.Scripts.Enums;

namespace Catan.Managers
{
    public class PurchaseManager : MonoBehaviour
    {
        public static PurchaseManager Instance { get; private set; }
        public PurchaseType SelectedPurchase { get; private set; } = PurchaseType.None;
        public event Action<PurchaseType> OnPurchaseChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetPurchase(PurchaseType purchase)
        {
            SelectedPurchase = purchase;
            Debug.Log($"[PurchaseManager] Set to {purchase}");
            OnPurchaseChanged?.Invoke(purchase);
        }

        public void ClearPurchase()
        {
            SelectedPurchase = PurchaseType.None;
        }
        public bool CanAfford(PurchaseType type, int[] current)
        {
            // super‑simple costs just to illustrate
            int cost = type == PurchaseType.Road ? 1 :
                       type == PurchaseType.Settlement ? 2 :
                       type == PurchaseType.City ? 3 : 0;

            return current[0] >= cost && current[1] >= cost; // lumber & wool for demo
        }

    }
}