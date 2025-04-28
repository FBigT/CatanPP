using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResourceButtonHandler : MonoBehaviour
{
    public string resourceName;
    public TextMeshProUGUI quantityText;
    private int quantity = 0;

    void Start()
    {
        if (quantityText != null)
        {
            quantityText.text = quantity.ToString();
        }
    }

    public void OnButtonClicked()
    {
        quantity++;
        if (quantityText != null)
        {
            quantityText.text = quantity.ToString();
        }
    }


    public int GetQuantity()
    {
        return quantity;
    }
}
