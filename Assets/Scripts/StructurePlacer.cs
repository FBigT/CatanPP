using UnityEngine;
using UnityEngine.UI;

public class StructurePlacer : MonoBehaviour
{
    public GameObject contextMenuPrefab; // Assign in Inspector
    private GameObject contextMenuInstance;

    private VertexPoint selectedVertex;

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // Right click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                var vertex = hit.collider.GetComponent<VertexPoint>();
                if (vertex != null)
                {
                    selectedVertex = vertex;
                    OpenContextMenu(Input.mousePosition);
                }
            }
        }
    }

    void OpenContextMenu(Vector3 screenPosition)
    {
        if (contextMenuInstance != null)
            Destroy(contextMenuInstance);

        contextMenuInstance = Instantiate(contextMenuPrefab, transform);
        contextMenuInstance.transform.position = screenPosition;

        // Assume each button is a child with a label and click handler
        Button[] buttons = contextMenuInstance.GetComponentsInChildren<Button>();
        foreach (var button in buttons)
        {
            string structureName = button.name;
            button.onClick.AddListener(() =>
            {
                selectedVertex?.Build(structureName); // e.g. "Settlement", "City"
                CloseMenu();
            });
        }
    }

    void CloseMenu()
    {
        if (contextMenuInstance != null)
            Destroy(contextMenuInstance);
    }
}
