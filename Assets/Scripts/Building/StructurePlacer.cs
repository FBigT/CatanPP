using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Assets.Scripts.Utils;  // For EndpointUtils, LocalStorageService

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
        // ===== Your ORIGINAL local logic =====
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

            // Physically place the structure in the local scene:
            connector.PlaceStructure(selectedPrefab);
            Debug.Log($"Placed {selectedPrefab.name} at {connector.transform.position}");

            // ===== NEW (Optional) BACKEND CALL =====
            // If you want to also inform the backend that a Settlement or Road was placed:
            // a) Distinguish corner vs edge from the prefab or connector:
            if (connector.Connection == Connector.ConnectionType.Corner)
            {
                // Suppose it's a Settlement => call the server.
                // (You'd need the real tileId + cornerIndex. Below is a placeholder.)
                int fakeTileId = 1;
                int cornerIndex = 0;
                StartCoroutine(PlaceSettlementOnServer(fakeTileId, cornerIndex));
            }
            else if (connector.Connection == Connector.ConnectionType.Edge)
            {
                // Suppose it's a Road => call the server.
                int fakeTileId = 1;
                int edgeIndex = 0;
                StartCoroutine(PlaceRoadOnServer(fakeTileId, edgeIndex));
            }
        }
    }

    // -------------------------------------------------------------------
    // Example: Telling the backend you placed a Settlement
    // -------------------------------------------------------------------
    private IEnumerator PlaceSettlementOnServer(int tileId, int cornerIndex)
    {
        // Build the URL
        string owner = LocalStorageService.GetString("username") ?? "UnknownUser";
        string url = EndpointUtils.PlaceStructure(owner, tileId, cornerIndex);

        // We do a POST request with an empty body; the data is in query params
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(new byte[0]);

        // Auth
        string token = LocalStorageService.GetString("token");
        if (!string.IsNullOrEmpty(token))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
        }

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Backend settlement placed: " + request.downloadHandler.text);
            // Optionally fetch resources from server if you want updated counts
        }
        else
        {
            Debug.LogError("Backend settlement placement failed: " + request.error);
        }
    }

    // -------------------------------------------------------------------
    // Example: Telling the backend you placed a Road
    // -------------------------------------------------------------------
    private IEnumerator PlaceRoadOnServer(int tileId, int edgeIndex)
    {
        string owner = LocalStorageService.GetString("username") ?? "UnknownUser";
        string url = EndpointUtils.PlaceRoad(owner, tileId, edgeIndex);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(new byte[0]);

        string token = LocalStorageService.GetString("token");
        if (!string.IsNullOrEmpty(token))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
        }

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Backend road placed: " + request.downloadHandler.text);
            // Optionally fetch resources from server
        }
        else
        {
            Debug.LogError("Backend road placement failed: " + request.error);
        }
    }
}
