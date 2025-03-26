using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HexConnectorGenerator : MonoBehaviour
{
    [SerializeField] SO_HexMetrics hexMetrics;
    [SerializeField] private float colliderRadius = 0.2f;
    private List<GameObject> cornerColliders = new List<GameObject>();

    private void OnValidate()
    {
        if (hexMetrics != null)
        {
            GenerateCornerColliders();
        }
    }

    private void GenerateCornerColliders()
    {
        foreach (var obj in cornerColliders)
        {
            if (obj) Destroy(obj);
        }
        cornerColliders.Clear();

        for (int i = 0; i < hexMetrics.Corners.Length; i++)
        {
            GameObject sphere = new GameObject($"StructureConnector_{i}");
            sphere.transform.SetParent(transform);
            sphere.transform.localPosition = hexMetrics.Corners[i];

            SphereCollider collider = sphere.AddComponent<SphereCollider>();
            collider.radius = colliderRadius;

            cornerColliders.Add(sphere);
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(HexConnectorGenerator))]
public class HexConnectorGeneratorEditor : Editor
{
    private HexConnectorGenerator hexGen;

    private void OnEnable()
    {
        hexGen = (HexConnectorGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Regenerate Connectors"))
        {

        }
    }
}
#endif