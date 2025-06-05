using UnityEngine;
using UnityEngine.EventSystems;

public class OnHoverPlaySound : MonoBehaviour, IOnHoverHandler
{
    [Header("Hover Sound")]
    public AudioClip hoverClip;

    public void OnHoverEnter()
    {
        if (hoverClip == null || UIAudioManager.Instance == null)
            return;

        UIAudioManager.Instance.PlaySFX(hoverClip);
    }

    public void OnHoverExit()
    {
        
    }
}
