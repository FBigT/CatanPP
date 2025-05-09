// Assets/Scripts/GameMode/CampaignGameMode.cs
using System.Collections.Generic;
using UnityEngine;
using Catan.TerrainGeneration;    // brings in NumberTokenIs, GetResource, GetCornerConnectors
using Catan.UI;                   // for TurnBannerUI
using Catan.Managers;             // for PurchaseManager
using Assets.Scripts.Enums;       // for PurchaseType

namespace Catan.GameMode
{
    /// <summary>
    /// Drives your Catan match: map generation, setup & play turns, resource distribution.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class CampaignGameMode : MonoBehaviour
    {
        public static CampaignGameMode Instance { get; private set; }

        [SerializeField] private MapGenerator mapGenerator;
        [Min(0)] public int botSeats = 3;

        private TurnManager _turns;
        private ResourceManager _bank;
        private List<PlayerState> _players;

        // 4-step free-build setup: Settlement → Road → Settlement → Road
        private readonly PurchaseType[] SetupSequence =
        {
            PurchaseType.Settlement,
            PurchaseType.Road,
            PurchaseType.Settlement,
            PurchaseType.Road
        };
        private int _setupStep = 0;

        void Awake()
        {
            // singleton pattern
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            BootNewMatch();
        }

        /// <summary>
        /// Initializes map, players, turn manager, and kicks off the first turn.
        /// </summary>
        private void BootNewMatch()
        {
            // 1) Generate a fresh map
            mapGenerator.GenerateMap();

            // 2) Create player states: you + N bots
            _players = new List<PlayerState> { new PlayerState(0, "You", false) };
            for (int i = 1; i <= botSeats; i++)
                _players.Add(new PlayerState(i, $"Bot {i}", true));

            // 3) Resource & turn managers
            _bank = new ResourceManager(_players);
            _turns = new TurnManager(_players);

            // 4) Subscribe to events
            _turns.OnTurnChanged += OnTurnChanged;
            Dice.DiceTotalHook.OnRollTotal += DistributeResources;

            // 5) Fire the very first turn
            OnTurnChanged(_turns.Current, _turns.Phase);
        }

        /// <summary>
        /// Handles both the free-build setup sequence and normal play turns.
        /// </summary>
        private void OnTurnChanged(PlayerState player, GamePhase phase)
        {
            if (phase == GamePhase.Setup)
            {
                // Determine which free item to build this step
                var toBuild = SetupSequence[_setupStep++];

                if (player.IsBot)
                {
                    Debug.Log($"[Campaign] Bot {player.Seat} auto-skips {toBuild}");
                    _turns.EndTurn();
                }
                else
                {
                    // Queue up your free build (zero-cost)
                    PurchaseManager.Instance.SetPurchase(toBuild);
                    TurnBannerUI.Instance?.ShowTurn(player, phase);
                }
            }
            else
            {
                // Normal play phase: just update the banner
                TurnBannerUI.Instance?.ShowTurn(player, phase);
            }
        }

        /// <summary>
        /// When a dice roll happens, distribute resources from matching hexes.
        /// </summary>
        private void DistributeResources(int roll)
        {
            foreach (var cell in Object.FindObjectsByType<HexCell>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None))
            {
                if (!cell.NumberTokenIs(roll)) continue;
                var res = cell.GetResource();

                foreach (var corner in cell.GetCornerConnectors())
                {
                    if (!corner.IsOccupied) continue;
                    var marker = corner.GetComponentInChildren<PlayerMarker>();
                    if (marker == null) continue;

                    _bank.Grant(_players[marker.OwnerSeat], res, 1);
                }
            }
        }

        /// <summary>
        /// Called by your UI’s End Turn button.
        /// </summary>
        public void EndTurn()
        {
            if (_turns.Current.IsBot)
                Debug.LogWarning("Cannot end turn: it’s not your turn!");
            else
                _turns.EndTurn();
        }

        /// <summary>
        /// Returns true if it’s currently the given seat’s turn.
        /// </summary>
        public bool IsPlayerTurn(int seat) => _turns.Current.Seat == seat;

        /// <summary>
        /// What phase are we in? (Setup vs Play)
        /// </summary>
        public GamePhase Phase => _turns.Phase;

        /// <summary>
        /// Alias property for UI code that expects “CurrentPlayer”.
        /// </summary>
        public PlayerState CurrentPlayer => _turns.Current;
    }
}
