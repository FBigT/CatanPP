// Assets/Scripts/GameMode/TurnManager.cs
using System;
using System.Collections.Generic;

namespace Catan.GameMode
{
    /// <summary>
    /// Keeps track of whose turn it is and which high‑level phase we are in.
    /// • Phase == Setup → two free Settlement‑&‑Road rounds (“snake” order).
    /// • Phase == Play  → normal dice / build / trade loop.
    /// </summary>
    public sealed class TurnManager
    {
        public event Action<PlayerState, GamePhase> OnTurnChanged;

        readonly List<PlayerState> _order;   // seat‑order 0,1,2…
        int _index;                         // whose turn right now?
        int _laps;                          // full rounds finished

        public GamePhase Phase { get; private set; } = GamePhase.Setup;
        public PlayerState Current => _order[_index];

        public TurnManager(List<PlayerState> turnOrder)
        {
            _order = turnOrder ?? throw new ArgumentNullException(nameof(turnOrder));
            _index = 0;
            _laps = 0;
            OnTurnChanged?.Invoke(Current, Phase);
        }

        /// <summary>Called by <see cref="CampaignGameMode.EndTurn"/>.</summary>
        public void EndTurn()
        {
            /* ─── advance seat index ─────────────────────────── */
            bool reverse = (Phase == GamePhase.Setup && _laps == 1); // 2nd lap runs backwards (classic snake rule)
            _index = reverse
                ? (_index - 1 + _order.Count) % _order.Count
                : (_index + 1) % _order.Count;

            /* wrapped around? → completed a lap ---------------- */
            if ((_index == 0 && !reverse) ||
                (_index == _order.Count - 1 && reverse))
                _laps++;

            /* switch phase after TWO complete laps ------------- */
            if (Phase == GamePhase.Setup && _laps >= 2)
                Phase = GamePhase.Play;

            OnTurnChanged?.Invoke(Current, Phase);
        }
    }
}
