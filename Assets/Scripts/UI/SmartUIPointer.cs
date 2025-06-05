using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Unity.VisualScripting;

[RequireComponent(typeof(RectTransform))]
public class SmartUIPointer : MonoBehaviour
{
    public static SmartUIPointer Instance { get; private set; }

    [Header("Weight Settings")]
    [SerializeField] private Transform weight;
    [SerializeField] private float weightOffset = 0;
    [SerializeField] private float weightSpringStrength = 10f;
    [SerializeField] private float weightDamping = 4f;

    [Header("Pointer Behavior")]
    public float moveSpeed = 8f;

    private Vector2 baseDirection = Vector2.right;

    [SerializeField] private RectTransform pointerRect;

    private Vector2 targetPosition = Vector2.zero;

    private Vector2 velocity = Vector2.zero;

    private Vector3 weightVelocity = Vector3.zero;
    private Vector3 desiredWeightPos;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        pointerRect = GetComponent<RectTransform>();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            Vector3 dir3 = new Vector3(baseDirection.x, baseDirection.y, 0);
            weight.position = dir3 * weightOffset + pointerRect.transform.position;
        }
    }

    private void Update()
    {
        Vector2 currentPos = pointerRect.localPosition;
        Vector2 newPos = Vector2.SmoothDamp(currentPos, targetPosition, ref velocity, 1f / moveSpeed);
        pointerRect.localPosition = newPos;

        Vector2 movement = newPos - currentPos;
        float moveMagnitude = movement.magnitude;

        Vector3 dir3 = new Vector3(baseDirection.x, baseDirection.y, 0);

        desiredWeightPos = pointerRect.position + dir3 * weightOffset;

        weight.transform.position = Spring(weight.transform.position, desiredWeightPos, ref weightVelocity, weightSpringStrength, weightDamping);

        Vector2 direction = -(Vector2)(weight.position - pointerRect.position);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        pointerRect.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private Vector3 Spring(Vector3 current, Vector3 target, ref Vector3 velocity, float springStrength, float damping)
    {
        velocity += (target - current) * springStrength * Time.deltaTime;
        velocity *= Mathf.Exp(-damping * Time.deltaTime);
        return current + velocity * Time.deltaTime;
    }

    public void MovePointer(Vector2 position) => targetPosition = position;
    public void SetBaseDirection(Vector2 dir) => baseDirection = dir.normalized;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (weight != null)
        {
            Gizmos.color = new Color(1, 0, 0, .5f);
            Gizmos.DrawSphere(weight.transform.position, 5f);
        }

        if (pointerRect == null)
            return;

        Gizmos.color = new Color(0, 0, 1, .5f);
        Vector3 dir3 = new Vector3(baseDirection.x, baseDirection.y, 0);
        Gizmos.DrawSphere(weightOffset * dir3 + pointerRect.position, 4f);

        Gizmos.color = Color.cyan;

        Vector3 velocityDir = new Vector3(velocity.x, velocity.y, 0);

        Vector3 pointerPosition = pointerRect.position;

        Gizmos.DrawLine(pointerPosition, pointerPosition + velocityDir * 50);
    }
#endif
}
