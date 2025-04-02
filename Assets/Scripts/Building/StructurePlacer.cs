using System.Collections.Generic;
using UnityEngine;

public class StructurePlacer : MonoBehaviour
{
    public LayerMask placementLayer;
    private OnStructureTabEvents uiManager;

    private void Start()
    {
        uiManager = FindAnyObjectByType<OnStructureTabEvents>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceStructure();
        }
    }

    private void TryPlaceStructure()
    {
        GameObject selectedPrefab = uiManager.GetSelectedStructure();
        if (selectedPrefab == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, placementLayer))
        {
            Connector connector = hit.collider.GetComponent<Connector>();
            if (connector == null)
            {
                Debug.Log("No valid connector found!");
                return;
            }

            if (!connector.CanPlaceStructure(selectedPrefab))
            {
                Debug.Log("Invalid placement: structure type does not match connection type or spot is occupied.");
                return;
            }

            connector.PlaceStructure(selectedPrefab);
            Debug.Log($"Placed {selectedPrefab.name} at {connector.transform.position}");
        }
    }
}
