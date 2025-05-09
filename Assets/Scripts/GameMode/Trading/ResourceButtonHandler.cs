using UnityEngine;
using TMPro;

namespace Assets.Scripts.GameMode.Trading
{
    public class ResourceButtonHandler : MonoBehaviour
    {
        public string resourceName;
        public TextMeshProUGUI quantityText;
        private int quantity = 0;

        void Start()
        {
            if (quantityText != null)
                quantityText.text = quantity.ToString();
        }

        public void OnButtonClicked()
        {
            quantity++;
            if (quantityText != null)
                quantityText.text = quantity.ToString();
        }

        public int GetQuantity() => quantity;
    }
}
