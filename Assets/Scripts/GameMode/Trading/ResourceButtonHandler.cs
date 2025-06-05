using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

namespace Assets.Scripts.GameMode.Trading
{
    public class ResourceButtonHandler : MonoBehaviour, IPointerClickHandler
    {
        public string resourceName;
        public TextMeshProUGUI quantityText;
        private int quantity = 0;

        void Start()
        {
            if (quantityText != null)
                quantityText.text = quantity.ToString();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // Left‐click: increment
                quantity++;
                quantityText.text = quantity.ToString();
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                // Right‐click: only decrement if > 0
                if (quantity > 0)
                {
                    quantity--;
                    quantityText.text = quantity.ToString();
                }
            }
        }

        /// <summary>
        /// Returns the current quantity clicked by the player.
        /// </summary>
        public int GetQuantity() => quantity;

        /// <summary>
        /// Zero out this button’s quantity and refresh its label.
        /// </summary>
        public void ResetQuantity()
        {
            quantity = 0;
            if (quantityText != null)
                quantityText.text = "0";
        }
    }
}
