// Assets/Scripts/Controllers/StructurePlacer.cs
//
// Drop‑in patch: fixes InvalidCastException when reading TileId / Index
// via reflection (they’re LONGs in the backend model).  No other logic
// changed; if you later decide to move placement 100 % server‑side you
// can still use this as the thin front‑end “click‑&‑POST” layer.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.Enums;
using Assets.Scripts.Utils;
using Catan.Placement;
using Catan.UI.LeftMenu;
using Catan.GameMode;
using Catan.Managers;
using Catan.UI;
using Assets.Scripts.GameMode.Trading;
using Assets.Scripts.GameMode.Trading.Models;

namespace Catan.Controllers
{
    public sealed class StructurePlacer : MonoBehaviour
    {
        #region Inspector
        [Header("Layers & Materials")]
        [SerializeField] private LayerMask placementLayer = ~0;
        [SerializeField] private Material highlightMaterial;
        #endregion

        private OnStructureTabEvents _ui;
        private GameObject _prefab;
        private bool _placing;
        private readonly List<Connector> _highlighted = new();

        /* reflection: Connector._currentStructure -------------------------------- */
        private static readonly FieldInfo _currentStructureField =
            typeof(Connector).GetField("_currentStructure",
                                       BindingFlags.NonPublic | BindingFlags.Instance);

        /* our session‑player id --------------------------------------------------- */
        private long _mySessionPlayerId;   // 0 → not yet initialised


        /* ====================================================================== */
        /*  life‑cycle                                                            */
        /* ====================================================================== */

        private void Awake()
        {
            TradingManager.OnPlayersLoaded += HandlePlayersLoaded;
        }

        private void Start()
        {
            _ui = FindFirstObjectByType<OnStructureTabEvents>();
            PurchaseManager.Instance.OnPurchaseChanged += BeginPlacement;
        }

        private void OnDestroy()
        {
            TradingManager.OnPlayersLoaded -= HandlePlayersLoaded;
        }


        /* ====================================================================== */
        /*  callbacks                                                             */
        /* ====================================================================== */

        private void HandlePlayersLoaded(List<SessionPlayerDto> players)
        {
            string me = LocalStorageService.GetString("username");
            var sp = players.FirstOrDefault(p => p.username == me);
            if (sp != null)
            {
                _mySessionPlayerId = sp.id;
                Debug.Log($"[StructurePlacer] my sessionPlayerId = {_mySessionPlayerId}");
            }
        }

        private void BeginPlacement(PurchaseType t)
        {
            if (t == PurchaseType.None || _ui == null) return;

            _prefab = _ui.GetSelectedStructure();
            _placing = _prefab != null;
            if (_placing) HighlightValid(t);
        }


        /* ====================================================================== */
        /*  highlight helpers                                                     */
        /* ====================================================================== */

        private void HighlightValid(PurchaseType t)
        {
            foreach (var c in FindObjectsByType<Connector>(FindObjectsSortMode.None))
            {
                bool ok =
                    (t == PurchaseType.Road && c.Connection == Connector.ConnectionType.Edge) ||
                    ((t == PurchaseType.Settlement || t == PurchaseType.City) &&
                     c.Connection == Connector.ConnectionType.Corner);

                if (ok && !c.IsOccupied)
                {
                    c.GetComponent<MeshRenderer>().material = highlightMaterial;
                    _highlighted.Add(c);
                }
            }
        }

        private void Unhighlight()
        {
            foreach (var c in _highlighted)
                c.GetComponent<MeshRenderer>().material = c.OriginalMaterial;
            _highlighted.Clear();
        }


        /* ====================================================================== */
        /*  Update & placement                                                    */
        /* ====================================================================== */

        private void Update()
        {
            if (_placing && Input.GetMouseButtonDown(0))
                TryPlace();
        }

        private void TryPlace()
        {
            if (!Camera.main) return;

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, placementLayer)) return;

            var conn = hit.collider.GetComponent<Connector>();
            if (conn == null || conn.IsOccupied || _prefab == null || !conn.CanPlaceStructure(_prefab))
                return;

            /* local spawn ------------------------------------------------------ */
            conn.PlaceStructure(_prefab);

            /* owner / cost tweaks --------------------------------------------- */
            if (_currentStructureField?.GetValue(conn) is GameObject go &&
                go.TryGetComponent<PlayerMarker>(out var marker))
            {
                bool myTurn = CampaignGameMode.Instance.IsPlayerTurn(0);
                marker.OwnerSeat = myTurn ? 0 : marker.OwnerSeat;

                if (myTurn && CampaignGameMode.Instance.Phase == GamePhase.Play)
                {
                    var player = CampaignGameMode.Instance.CurrentPlayer;
                    var cost = Costs.Get(PurchaseManager.Instance.SelectedPurchase);
                    for (int i = 0; i < cost.Length; i++)
                        player.Resources[i] -= cost[i];

                    TopBarUI.Instance.SendMessage("SetValues", player.Resources);
                    _ui.UpdateAffordability(player.Resources);
                }
            }

            /* notify game‑mode -------------------------------------------------- */
            CampaignGameMode.Instance.NotifyFreeStructurePlaced(
                PurchaseManager.Instance.SelectedPurchase);

            if (CampaignGameMode.Instance.Phase != GamePhase.Setup)
                PurchaseManager.Instance.Clear();

            _placing = false;
            Unhighlight();

            /* backend ---------------------------------------------------------- */
            StartCoroutine(SendToServer(conn));
        }


        /* ====================================================================== */
        /*  Backend POST                                                          */
        /* ====================================================================== */

        private System.Collections.IEnumerator SendToServer(Connector conn)
        {
            if (_mySessionPlayerId == 0)
            {
                Debug.LogWarning("[StructurePlacer] sessionPlayerId not yet initialised – skipping backend call");
                yield break;
            }

            /* ---- robustly fetch tile/edge ids – can be *long* coming           */
            long tileId = 0;
            int subIndex = 0;

            var tileProp = conn.GetType().GetProperty("TileId");
            if (tileProp != null && tileProp.GetValue(conn) != null)
                tileId = Convert.ToInt64(tileProp.GetValue(conn));

            var idxProp = conn.GetType().GetProperty("Index");
            if (idxProp != null && idxProp.GetValue(conn) != null)
                subIndex = Convert.ToInt32(idxProp.GetValue(conn));

            if (tileId == 0)
            {
                Debug.LogWarning("[StructurePlacer] Connector reports tileId=0 – backend call cancelled.");
                yield break;
            }

            string baseUrl = EndpointUtils.BaseUrl.TrimEnd('/');

            string url = conn.Connection == Connector.ConnectionType.Corner
                ? $"{baseUrl}/place/structure?sessionPlayerId={_mySessionPlayerId}&tileId={tileId}&cornerIndex={subIndex}"
                : $"{baseUrl}/place/road?sessionPlayerId={_mySessionPlayerId}&tileId={tileId}&edgeIndex={subIndex}";

            using UnityWebRequest req = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Array.Empty<byte>()),
                downloadHandler = new DownloadHandlerBuffer()
            };

            if (LocalStorageService.GetString("token") is { } tok && !string.IsNullOrEmpty(tok))
                req.SetRequestHeader("Authorization", tok);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
                Debug.LogError($"[StructurePlacer] {req.responseCode} {req.error}\n{req.downloadHandler.text}");
            else
                Debug.Log("[StructurePlacer] ✓ backend confirmed placement");
        }
    }
}
