using UnityEngine;

[CreateAssetMenu(fileName = "NewCornerMetric", menuName = "Hex/Corner")]
public class SO_CornerMetrics : ScriptableObject
{
    [SerializeField] private float colliderRadius = 10f;

    public float ColliderRadius => colliderRadius;
}
