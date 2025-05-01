// Assets/Scripts/GameMode/Costs.cs
using Assets.Scripts.Enums;

namespace Catan.GameMode
{
    /// <summary>8-element arrays must be in TopBarUI order (lumber,wool,grain,bricks,ore,gold,silver,obsidian).</summary>
    public static class Costs
    {
        public static readonly int[][] Table =
        {
            /* None        */ new int[8],
            /* Settlement  */ new[]{ 1,1,1,1,0,0,0,0 },
            /* Road        */ new[]{ 1,1,0,1,0,0,0,0 },
            /* City        */ new[]{ 0,0,2,0,3,0,0,0 },
            /* DevCard     */ new[]{ 0,1,1,0,1,0,0,0 }
        };

        public static int[] Get(PurchaseType t) => Table[(int)t];
    }
}
