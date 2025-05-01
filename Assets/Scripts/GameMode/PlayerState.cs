
using System;

namespace Catan.GameMode
{
    [Serializable]
    public sealed class PlayerState
    {
        public int Seat { get; }
        public string Name { get; }
        public bool IsBot { get; }

        public int[] Resources = new int[8];

        public string DisplayName => Name;

        public PlayerState(int seat, string name, bool bot)
        {
            Seat = seat;
            Name = name;
            IsBot = bot;
        }

        public void Add(ResourceType type, int qty = 1)
        {
            int idx = (int)type;
            if (idx < 0 || idx >= Resources.Length) return;
            Resources[idx] += qty;
        }
    }
}

