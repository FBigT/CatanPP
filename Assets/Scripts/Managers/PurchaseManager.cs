using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class PurchaseManager : MonoBehaviour
    {
        public static PurchaseManager Instance { get; private set; }

        public PurchaseType SelectedPurchase { get; private set; } = PurchaseType.None;

        private void Awake()
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
            Debug.Log($"PurchaseManager: Set selected purchase to {purchase}");
        }

        public void ClearPurchase()
        {
            SelectedPurchase = PurchaseType.None;
        }
    }
}
