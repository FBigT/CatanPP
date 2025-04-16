using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;    // ← Required for UnityWebRequest, UploadHandlerRaw, DownloadHandlerBuffer
using Assets.Scripts.Enums;      // PurchaseType
using Catan.Managers;            // PurchaseManager
using Catan.Placement;           // Connector
using Catan.UI;                  // TopBarUI
using Assets.Scripts.Utils;      // EndpointUtils, LocalStorageService
using Catan.UI.LeftMenu;         // OnStructureTabEvents

namespace Catan.Controllers
{
    public class StructurePlacer : MonoBehaviour
    {
        [Header("Board Raycast")]
        [SerializeField] private LayerMask placementLayer;
        [Header("Highlight Material")]
        [SerializeField] private Material highlightMaterial;

        private OnStructureTabEvents uiManager;
        private GameObject prefabToPlace;
        private bool isPlacing;
        private readonly List<Connector> highlighted = new List<Connector>();

        private void Start()
        {
            uiManager = FindObjectOfType<OnStructureTabEvents>();
            PurchaseManager.Instance.OnPurchaseChanged += BeginPlacement;
        }

        private void BeginPlacement(PurchaseType type)
        {
            if (type == PurchaseType.None) return;

            prefabToPlace = uiManager.GetSelectedStructure();
            isPlacing = prefabToPlace != null;
            HighlightValid(type);
        }

        private void Update()
        {
            if (!isPlacing) return;

            if (Input.GetMouseButtonDown(0))
                TryPlace();
        }

        private void HighlightValid(PurchaseType type)
        {
            foreach (var c in FindObjectsOfType<Connector>())
            {
                bool ok =
                    (type == PurchaseType.Road && c.Connection == Connector.ConnectionType.Edge)
                 || ((type == PurchaseType.Settlement || type == PurchaseType.City)
                     && c.Connection == Connector.ConnectionType.Corner);

                if (ok && !c.IsOccupied)
                {
                    c.GetComponent<MeshRenderer>().material = highlightMaterial;
                    highlighted.Add(c);
                }
            }
        }

        private void Unhighlight()
        {
            foreach (var c in highlighted)
            {
                c.GetComponent<MeshRenderer>().material = c.OriginalMaterial;
            }
            highlighted.Clear();
        }

        private void TryPlace()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, placementLayer))
                return;

            var conn = hit.collider.GetComponent<Connector>();
            if (conn == null || conn.IsOccupied || !conn.CanPlaceStructure(prefabToPlace))
                return;

            conn.PlaceStructure(prefabToPlace);
            StartCoroutine(SendToServer(conn));

            isPlacing = false;
            Unhighlight();
            PurchaseManager.Instance.ClearPurchase();
            TopBarUI.Instance.RefreshResources();
        }

        private IEnumerator SendToServer(Connector conn)
        {
            string owner = LocalStorageService.GetString("username") ?? "tester";
            // choose the correct endpoint
            string url = (conn.Connection == Connector.ConnectionType.Corner)
                ? EndpointUtils.PlaceStructure(owner, /*tileId*/1, /*cornerIndex*/0)
                : EndpointUtils.PlaceRoad(owner, /*tileId*/1, /*edgeIndex*/0);

            using var req = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(new byte[0]),
                downloadHandler = new DownloadHandlerBuffer()
            };

            string token = LocalStorageService.GetString("token");
            if (!string.IsNullOrEmpty(token))
                req.SetRequestHeader("Authorization", $"Bearer {token}");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
                Debug.LogError($"[StructurePlacer] Error placing structure: {req.error}");
        }
    }
}
