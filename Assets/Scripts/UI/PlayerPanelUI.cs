using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerPanelUI : MonoBehaviour
{
    [SerializeField] private Image panelImage;
    [SerializeField] private Image highlightImage;
    [SerializeField] private TMP_Text playerNameText;

    private Color playerColor;
    private bool isActive = false;
    private float hue = 0f;

    private void Update()
    {
        if (isActive && highlightImage != null)
        {
            hue += Time.deltaTime * 0.2f;
            if (hue > 1f) hue -= 1f;

            Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);
            highlightImage.color = rainbowColor;
        }
    }

    public void SetColor(Color color)
    {
        playerColor = color;
        panelImage.color = new Color(color.r, color.g, color.b, 0.5f);
    }

    public void SetName(string playerName)
    {
        if (playerNameText != null)
            playerNameText.text = playerName;
    }

    public void SetActive(bool active)
    {
        isActive = active;

        if (!active && highlightImage != null)
        {
            highlightImage.color = Color.clear;
        }
    }

    public Color GetColor() => playerColor;
}
