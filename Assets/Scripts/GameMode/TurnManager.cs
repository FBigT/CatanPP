using System;
using System.Collections.Generic;

namespace Catan.GameMode
{
    /// <summary>Keeps track of whose turn it is and raises an event when it changes.</summary>
    public sealed class TurnManager
    {
        public event Action<PlayerState, GamePhase> OnTurnChanged;

        readonly List<PlayerState> _order;
        int _index;

        public GamePhase Phase { get; private set; } = GamePhase.Setup;
        public PlayerState Current => _order[_index];

        public TurnManager(List<PlayerState> turnOrder)
        {
            _order = turnOrder ?? throw new ArgumentNullException(nameof(turnOrder));
            _index = 0;
            OnTurnChanged?.Invoke(Current, Phase);
        }

        /// <remarks>
        /// After every player has placed their two free settlements we switch
        /// from <see cref="GamePhase.Setup"/> to <see cref="GamePhase.Play"/>.
        /// </remarks>
        public void EndTurn()
        {
            _index = (_index + 1) % _order.Count;

            if (Phase == GamePhase.Setup && _index == 0)
                Phase = GamePhase.Play;

            OnTurnChanged?.Invoke(Current, Phase);
        }
    }
}
