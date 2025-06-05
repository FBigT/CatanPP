// Assets/Scripts/GameMode/ResourceManager.cs
using System.Collections.Generic;
using UnityEngine;
using Catan.UI;
using Catan.UI.LeftMenu;

namespace Catan.GameMode
{
    public class ResourceManager
    {
        readonly Dictionary<int, PlayerState> _bySeat = new();

        public ResourceManager(IEnumerable<PlayerState> players)
        {
            foreach (var p in players) _bySeat[p.Seat] = p;
        }

        public void Grant(PlayerState p, ResourceType t, int qty = 1)
        {
            p.Add(t, qty);

            if (p.Seat == 0 && TopBarUI.Instance != null)
            {
                TopBarUI.Instance.SendMessage("SetValues", p.Resources);
                var ui = Object.FindFirstObjectByType<OnStructureTabEvents>();
                if (ui != null)
                    ui.UpdateAffordability(p.Resources);
            }
        }
    }
}
