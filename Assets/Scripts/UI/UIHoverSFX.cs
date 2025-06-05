using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverSFX : MonoBehaviour, IPointerEnterHandler
{
    [Header("Hover Sound")]
    public AudioClip hoverClip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverClip == null || UIAudioManager.Instance == null)
            return;

        UIAudioManager.Instance.PlaySFX(hoverClip);
    }
}
