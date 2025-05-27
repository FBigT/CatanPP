using System.Xml.Linq;
using UnityEngine;

public class VertexPoint : MonoBehaviour
{
    public Vector3 Position => transform.position;

    public object owner;
    public GameObject buildingRoot; // A parent GameObject holding all structure models (disabled)

    public void Build(string structureName)
    {
        if (owner != null) return; // Already built

        owner = "debug"; // placeholder

        foreach (Transform child in buildingRoot.transform)
        {
            child.gameObject.SetActive(child.name == structureName);
        }

        Debug.Log($"Built: {structureName} at {name}");
    }
}
