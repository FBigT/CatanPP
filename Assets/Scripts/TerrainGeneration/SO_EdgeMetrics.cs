using UnityEngine;

[CreateAssetMenu(fileName = "NewEdgeMetric", menuName = "Hex/Edge")]
public class SO_EdgeMetrics : ScriptableObject
{
    [SerializeField] private float colliderRadius = 10f;

    public float ColliderRadius => colliderRadius;
}
