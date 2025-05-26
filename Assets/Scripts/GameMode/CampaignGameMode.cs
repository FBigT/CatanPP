// Assets/Scripts/GameMode/CampaignGameMode.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;                            // for Contains()
using UnityEngine;
using Object = UnityEngine.Object;             // disambiguate Object.FindObjectsOfType
using Catan.TerrainGeneration;                 // NumberTokenIs(), GetResource(), GetCornerConnectors()
using Catan.UI;                                // TurnBannerUI, TopBarUI
using Catan.Managers;                          // ResourceManager, PurchaseManager
using Assets.Scripts.Enums;                    // PurchaseType
using Catan.Placement;                         // Connector, PlayerMarker
using DiceRNG = Catan.GameMode.Dice.Dice;      // handy alias

namespace Catan.GameMode
{
    [DefaultExecutionOrder(-100)]
    public class CampaignGameMode : MonoBehaviour
    {
        public static CampaignGameMode Instance { get; private set; }

        [Header("Scene refs & tuning")]
        [SerializeField] private MapGenerator mapGenerator;
        [Min(0)] public int botSeats = 3;
        [SerializeField] private float botThinkTime = .6f;

        /// <summary>
        /// Fired to gate the buy-buttons UI. (isMyTurn, inSetupPhase)
        /// </summary>
        public event Action<bool, bool> TurnChangedForUI;

        // ── core managers & state ───────────────────────────────────
        private TurnManager _turns;
        private ResourceManager _bank;
        private List<PlayerState> _players;

        // ── setup-phase helper ───────────────────────────────────────
        private bool _awaitFreeRoad;

        // ── so we can cancel mid-routine ─────────────────────────────
        private Coroutine _botRoutine;

        // to detect transition out of setup
        private GamePhase _lastPhase;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start() => BootNewMatch();

        private void BootNewMatch()
        {
            if (_botRoutine != null) StopCoroutine(_botRoutine);

            // 1) generate the map
            mapGenerator.GenerateMap();

            // 2) create players (seat 0 = human)
            _players = new List<PlayerState> { new PlayerState(0, "You", false) };
            for (int i = 1; i <= botSeats; i++)
                _players.Add(new PlayerState(i, $"Bot {i}", true));

            // 3) set up managers
            _bank = new ResourceManager(_players);
            _turns = new TurnManager(_players);

            // 4) wire up events
            _turns.OnTurnChanged += OnTurnChanged;
            DiceRNG.DiceTotalHook.OnRollTotal += DistributeResources;

            // 5) start the first turn
            OnTurnChanged(_turns.Current, _turns.Phase);
        }

        private void OnTurnChanged(PlayerState p, GamePhase phase)
        {
            // fire UI gate
            TurnChangedForUI?.Invoke(p.Seat == 0, phase == GamePhase.Setup);

            // update the banner
            TurnBannerUI.Instance?.ShowTurn(p, phase);

            // if we just left setup, grant the two free settlements’ resources
            if (_lastPhase == GamePhase.Setup && phase == GamePhase.Play)
            {
                GrantSetupResources();
                TopBarUI.Instance?.RefreshResources();
            }
            _lastPhase = phase;

            if (phase == GamePhase.Setup)
            {
                if (p.IsBot)
                {
                    // bots instantly skip both freebies
                    _turns.EndTurn();
                }
                else
                {
                    // human setup: place settlement first, then road
                    _awaitFreeRoad = true;
                    PurchaseManager.Instance.SetPurchase(PurchaseType.Settlement);
                }
                return; // no dice roll in setup
            }

            // ── Play phase ──
            PurchaseManager.Instance.Clear(); // turn off any free-build UI

            if (p.IsBot)
            {
                if (_botRoutine != null) StopCoroutine(_botRoutine);
                _botRoutine = StartCoroutine(BotPlayRoutine());
            }
            else
            {
                DiceRNG.Roll(); // human rolls to start
            }
        }

        private IEnumerator BotPlayRoutine()
        {
            DiceRNG.Roll();
            yield return new WaitForSeconds(botThinkTime);
            _turns.EndTurn();
        }

        /// <summary>
        /// Called by StructurePlacer after each free placement in setup.
        /// </summary>
        public void NotifyFreeStructurePlaced(PurchaseType placed)
        {
            if (_turns.Phase != GamePhase.Setup || CurrentPlayer.IsBot) return;

            if (_awaitFreeRoad && placed == PurchaseType.Settlement)
            {
                // queue the free road
                _awaitFreeRoad = false;
                PurchaseManager.Instance.SetPurchase(PurchaseType.Road);
            }
            else if (placed == PurchaseType.Road)
            {
                // done with this setup turn
                _turns.EndTurn();
            }
        }

        /// <summary>
        /// Once setup ends, grant 1 resource from each adjacent tile of your two settlements.
        /// </summary>
        private void GrantSetupResources()
        {
            // grab all corners & all cells
            var corners = Object.FindObjectsOfType<Connector>();
            var cells = Object.FindObjectsOfType<HexCell>();

            // for each corner you occupy as seat 0
            foreach (var corner in corners)
            {
                if (corner.Connection != Connector.ConnectionType.Corner || !corner.IsOccupied)
                    continue;

                var marker = corner.GetComponentInChildren<PlayerMarker>();
                if (marker == null || marker.OwnerSeat != 0) continue;

                // for each hex whose corner-list contains this connector
                foreach (var cell in cells)
                {
                    var adj = cell.GetCornerConnectors();
                    if (adj.Contains(corner))
                    {
                        var res = cell.GetResource();
                        _bank.Grant(_players[0], res, 1);
                    }
                }
            }
        }

        private void DistributeResources(int roll)
        {
            // parameterless FindObjectsOfType<T>() only
            foreach (var cell in Object.FindObjectsOfType<HexCell>())
            {
                if (!cell.NumberTokenIs(roll) || cell.HasRobber()) continue;
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
        /// Give the current player +5 of each resource type,
        /// update the TopBar HUD, and re-fire the UI gate so the
        /// structure buttons re-check affordability.
        /// </summary>
        public void AwardStartTurnResources()
        {
            var p = CurrentPlayer;
            for (int i = 0; i < p.Resources.Length; i++)
                p.Resources[i] += 5;

            // 1) push the new values to TopBarUI (fires OnResourcesChanged)
            TopBarUI.Instance.SendMessage("SetValues", p.Resources);

            // 2) re-fire the structure-tab gate → UpdateAffordability()
            TurnChangedForUI?.Invoke(true, false);
        }
            /// <summary>
    /// Test helper: re-fire the buy-buttons UI gate from tests.
    /// </summary>
    public void SimulateUiGate(bool isMyTurn, bool inSetupPhase)
    {
        TurnChangedForUI?.Invoke(isMyTurn, inSetupPhase);
    }

        /// <summary>Called by the End Turn button.</summary>
        public void EndTurn()
        {
            if (!CurrentPlayer.IsBot)
            {
                // advance turn…
                _turns.EndTurn();

                // …then award +5 of each to the new active player
                AwardStartTurnResources();
            }
        }

        // ── Public API ───────────────────────────────────────────
        public bool IsPlayerTurn(int seat) => _turns.Current.Seat == seat;
        public GamePhase Phase => _turns.Phase;
        public PlayerState CurrentPlayer => _turns.Current;
    }
}
