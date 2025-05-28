using UnityEngine;

[RequireComponent(typeof(Collider))]
public class OnHover : MonoBehaviour
{
    [Header("Hover Animation Settings")]
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);
    public float animationSpeed = 5f;
    public Color hoverColor = Color.yellow;
    public bool useColorTint = false;

    private Vector3 originalScale;
    private Color originalColor;
    private bool isHovered = false;
    private Renderer targetRenderer;

    void Start()
    {
        originalScale = transform.localScale;
        targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer != null)
        {
            originalColor = targetRenderer.material.color;
        }
    }

    void Update()
    {
        DetectHover();

        // Animate scale
        Vector3 targetScale = isHovered ? hoverScale : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);

        // Animate color
        if (useColorTint && targetRenderer != null)
        {
            Color targetColor = isHovered ? hoverColor : originalColor;
            targetRenderer.material.color = Color.Lerp(targetRenderer.material.color, targetColor, Time.deltaTime * animationSpeed);
        }
    }

    void DetectHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        isHovered = false;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isHovered = true;
            }
        }
    }
}
