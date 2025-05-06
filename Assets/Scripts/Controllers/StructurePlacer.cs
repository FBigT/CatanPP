// Assets/Scripts/Controllers/StructurePlacer.cs
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.Enums;      // PurchaseType
using Assets.Scripts.Utils;      // LocalStorageService
using Catan.Placement;           // Connector
using Catan.UI.LeftMenu;         // OnStructureTabEvents
using Catan.GameMode;            // CampaignGameMode, GamePhase
using Catan.Managers;            // PurchaseManager
using Catan.UI;                  // TopBarUI

namespace Catan.Controllers
{
    /// <summary>Handles highlighting & placing Roads / Settlements / Cities.</summary>
    public sealed class StructurePlacer : MonoBehaviour
    {
        [Header("Layers & Materials")]
        [SerializeField] private LayerMask placementLayer = ~0;
        [SerializeField] private Material highlightMaterial;

        private OnStructureTabEvents _ui;
        private GameObject _prefab;
        private bool _placing;
        private readonly List<Connector> _highlighted = new();

        // Track whether we've already placed our one Settlement and one Road in Setup
        private bool _settlementPlaced;
        private bool _roadPlaced;

        static readonly FieldInfo _currentStructureField = typeof(Connector)
            .GetField("currentStructure", BindingFlags.NonPublic | BindingFlags.Instance);

        void Start()
        {
            _ui = FindObjectOfType<OnStructureTabEvents>();
            PurchaseManager.Instance.OnPurchaseChanged += BeginPlacement;

            // reset at start of match
            _settlementPlaced = false;
            _roadPlaced = false;
        }

        void BeginPlacement(PurchaseType t)
        {
            // nothing selected, or UI not ready?
            if (t == PurchaseType.None || _ui == null) return;

            // during setup, enforce one‐and‐only‐one
            if (CampaignGameMode.Instance.Phase == GamePhase.Setup)
            {
                if (t == PurchaseType.Settlement && _settlementPlaced) return;
                if (t == PurchaseType.Road && _roadPlaced) return;
            }

            _prefab = _ui.GetSelectedStructure();
            _placing = _prefab != null;
            if (_placing) HighlightValid(t);
        }

        void Update()
        {
            if (_placing && Input.GetMouseButtonDown(0))
                TryPlace();
        }

        void HighlightValid(PurchaseType t)
        {
            foreach (var c in FindObjectsOfType<Connector>())
            {
                bool ok =
                    (t == PurchaseType.Road && c.Connection == Connector.ConnectionType.Edge) ||
                    ((t == PurchaseType.Settlement || t == PurchaseType.City)
                     && c.Connection == Connector.ConnectionType.Corner);

                if (ok && !c.IsOccupied)
                {
                    c.GetComponent<MeshRenderer>().material = highlightMaterial;
                    _highlighted.Add(c);
                }
            }
        }

        void Unhighlight()
        {
            foreach (var c in _highlighted)
                c.GetComponent<MeshRenderer>().material = c.OriginalMaterial;
            _highlighted.Clear();
        }

        void TryPlace()
        {
            if (!Camera.main) return;

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, placementLayer)) return;

            var conn = hit.collider.GetComponent<Connector>();
            if (conn == null || conn.IsOccupied || !_prefab || !conn.CanPlaceStructure(_prefab))
                return;

            // 1) Spawn locally
            conn.PlaceStructure(_prefab);

            // 2) Mark owner + deduct cost if it's your turn
            if (_currentStructureField.GetValue(conn) is GameObject placed)
            {
                var marker = placed.GetComponent<PlayerMarker>();
                if (marker != null)
                {
                    bool isMyTurn = CampaignGameMode.Instance.IsPlayerTurn(0);
                    marker.OwnerSeat = isMyTurn ? 0 : marker.OwnerSeat;

                    if (isMyTurn)
                    {
                        // Deduct the cost
                        var player = CampaignGameMode.Instance.CurrentPlayer;
                        var cost = Costs.Get(PurchaseManager.Instance.SelectedPurchase);
                        for (int i = 0; i < cost.Length; i++)
                            player.Resources[i] -= cost[i];

                        TopBarUI.Instance.SendMessage("SetValues", player.Resources);
                        _ui.UpdateAffordability(player.Resources);

                        // track that we’ve placed our one Settlement/Road in Setup
                        var p = PurchaseManager.Instance.SelectedPurchase;
                        if (CampaignGameMode.Instance.Phase == GamePhase.Setup)
                        {
                            if (p == PurchaseType.Settlement) _settlementPlaced = true;
                            if (p == PurchaseType.Road) _roadPlaced = true;
                        }
                    }
                }
            }

            // 3) Send to server (using your existing placeholders)
            StartCoroutine(SendToServer(conn));

            // 4) Cleanup
            _placing = false;
            Unhighlight();
            PurchaseManager.Instance.Clear();
            TopBarUI.Instance.RefreshResources();

            // 5) auto-advance in setup phase
            if (CampaignGameMode.Instance.Phase == GamePhase.Setup)
                CampaignGameMode.Instance.EndTurn();
        }

        IEnumerator SendToServer(Connector conn)
        {
            // NOTE: you can swap the "1, 0" for real tileId & cornerIndex once your back end agrees.
            string owner = LocalStorageService.GetString("username") ?? "tester";
            string url = conn.Connection == Connector.ConnectionType.Corner
                         ? EndpointUtils.PlaceStructure(owner, 1, 0)
                         : EndpointUtils.PlaceRoad(owner, 1, 0);

            using var req = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(System.Array.Empty<byte>()),
                downloadHandler = new DownloadHandlerBuffer()
            };
            if (LocalStorageService.GetString("token") is { } tok && tok != "")
                req.SetRequestHeader("Authorization", tok);

            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
                Debug.LogError($"[StructurePlacer] {req.error}");
        }
    }
}
