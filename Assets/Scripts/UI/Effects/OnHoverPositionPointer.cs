using TMPro;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(RectTransform))]
public class OnHoverPositionPointer : MonoBehaviour, IOnHoverHandler
{
    public enum PointerDirection { Top, Bottom, Left, Right }

    public PointerDirection direction = PointerDirection.Right;
    public float padding = 10f;
    [SerializeField, Range(0, 360)] float pointerDirectionAngle = 90f;

    public bool isPointerStartingPoint = false;

    private RectTransform elementRect;
    private RectTransform canvasRect;

    private void Awake()
    {
        elementRect = GetComponent<RectTransform>();
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            canvasRect = canvas.GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (isPointerStartingPoint)
            OnHoverEnter();
    }

    public void OnHoverEnter()
    {
        if (SmartUIPointer.Instance != null && canvasRect != null)
        {
            Vector2 offsetPos = GetPointerPosition();
            SmartUIPointer.Instance.SetBaseDirection(CalculateRotationVector(pointerDirectionAngle));
            SmartUIPointer.Instance.MovePointer(offsetPos);
        }
    }

    public void OnHoverExit() { }

    private Vector2 GetPointerPosition()
    {
        Vector3 worldPos = elementRect.position;
        Vector2 localPos = (Vector2)canvasRect.InverseTransformPoint(worldPos);

        Vector2 size = elementRect.rect.size;
        Vector2 offset = Vector2.zero;

        switch (direction)
        {
            case PointerDirection.Top: offset = new Vector2(0, size.y / 2 + padding); break;
            case PointerDirection.Bottom: offset = new Vector2(0, -size.y / 2 - padding); break;
            case PointerDirection.Left: offset = new Vector2(-size.x / 2 - padding, 0); break;
            case PointerDirection.Right: offset = new Vector2(size.x / 2 + padding, 0); break;
        }

        return localPos + offset;
    }

    private Vector2 CalculateRotationVector(float value)
    {
        float radValue = Mathf.Deg2Rad * value;
        return new Vector2(-Mathf.Sin(radValue), Mathf.Cos(radValue));
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            elementRect = GetComponent<RectTransform>();

        if (elementRect == null)
            return;

        Vector3 worldPos = elementRect.position;

        Vector2 size = Vector2.Scale(elementRect.rect.size, elementRect.lossyScale);

        Vector2 offset = Vector2.zero;

        switch (direction)
        {
            case PointerDirection.Top:
                offset = new Vector2(0, size.y / 2 + padding);
                break;
            case PointerDirection.Bottom:
                offset = new Vector2(0, -size.y / 2 - padding);
                break;
            case PointerDirection.Left:
                offset = new Vector2(-size.x / 2 - padding, 0);
                break;
            case PointerDirection.Right:
                offset = new Vector2(size.x / 2 + padding, 0);
                break;
        }

        Vector3 gizmoPos = worldPos + (Vector3)offset;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(gizmoPos, size);
        Gizmos.DrawLine(worldPos, gizmoPos);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(gizmoPos, (Vector3)CalculateRotationVector(pointerDirectionAngle).normalized * 100f + gizmoPos);
    }
#endif

}


#if UNITY_EDITOR

[CustomEditor(typeof(OnHoverPositionPointer))]
public class OnHoverPositionPointerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
#endif