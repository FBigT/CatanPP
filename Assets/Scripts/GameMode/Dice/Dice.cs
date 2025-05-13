using System;

namespace Catan.GameMode.Dice
{
    /// <summary>Simple 2 d6 roller used by the rules engine.</summary>
    public static class Dice
    {
        static readonly Random _rng = new();

        public static int LastTotal { get; private set; }

        public static event Action<int> OnRollTotal;

        public static int Roll()
        {
            int a = _rng.Next(1, 7);
            int b = _rng.Next(1, 7);
            LastTotal = a + b;
            OnRollTotal?.Invoke(LastTotal);
            return LastTotal;
        }

        /// <remarks>
        /// Tiny shim so existing code can keep the old “Dice.DiceTotalHook”
        /// subscription style – no other files need edits.
        /// </remarks>
        public static class DiceTotalHook
        {
            public static event Action<int> OnRollTotal
            {
                add => Dice.OnRollTotal += value;
                remove => Dice.OnRollTotal -= value;
            }
        }
    }
}
