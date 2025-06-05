using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform), typeof(AudioSource))]
public class PointerHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Pointer Settings")]
    public Vector3 targetOffset = new Vector3(100, 0, 0); // Relative offset on hover
    public float moveSpeed = 5f;
    public float swayAmount = 15f; // degrees
    public float swaySpeed = 5f;

    [Header("Sound")]
    public AudioClip hoverSound;

    private RectTransform rectTransform;
    private AudioSource audioSource;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 targetPosition;
    private bool isHovered = false;
    private float swayTime = 0f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        audioSource = GetComponent<AudioSource>();

        initialPosition = rectTransform.anchoredPosition;
        initialRotation = rectTransform.rotation;
        targetPosition = initialPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetPosition = initialPosition + targetOffset;
        isHovered = true;

        if (hoverSound != null)
            audioSource.PlayOneShot(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetPosition = initialPosition;
        isHovered = false;
    }

    private void Update()
    {
        // Smooth position interpolation
        rectTransform.anchoredPosition = Vector3.Lerp(rectTransform.anchoredPosition, targetPosition, Time.deltaTime * moveSpeed);

        // Rotational sway while hovered
        if (isHovered)
        {
            swayTime += Time.deltaTime * swaySpeed;
            float swayAngle = Mathf.Sin(swayTime) * swayAmount;
            rectTransform.rotation = Quaternion.Euler(0f, 0f, swayAngle);
        }
        else
        {
            swayTime = 0f;
            rectTransform.rotation = Quaternion.Lerp(rectTransform.rotation, initialRotation, Time.deltaTime * moveSpeed);
        }
    }
}
