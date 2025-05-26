using UnityEngine;

public class VertexPoint : MonoBehaviour
{
    public Vector3 Position => transform.position;

    public object owner;

    public GameObject buildingModel;

    public void Build(object player)
    {
        if (owner != null) return;

        owner = player;

        if (buildingModel != null)
        {
            buildingModel.SetActive(true);

            var renderer = buildingModel.GetComponent<Renderer>();
        }
    }
}
