using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.TradingReasources.Models
{
    [Serializable]
    public class ResourceGroup
    {
        public int brick;
        public int crystal;
        public int ore;
        public int rice;
        public int sheep;
        public int silver;
        public int gold;
        public int wood;

        /// <summary>
        /// Creates a ResourceGroup from parallel lists of names and quantities.
        /// </summary>
        public static ResourceGroup FromLists(List<string> names, List<int> qty)
        {
            var rg = new ResourceGroup();
            for (int i = 0; i < names.Count && i < qty.Count; i++)
            {
                switch (names[i].ToLower())
                {
                    case "brick": rg.brick = qty[i]; break;
                    case "crystal": rg.crystal = qty[i]; break;
                    case "ore": rg.ore = qty[i]; break;
                    case "rice": rg.rice = qty[i]; break;
                    case "sheep": rg.sheep = qty[i]; break;
                    case "silver": rg.silver = qty[i]; break;
                    case "gold": rg.gold = qty[i]; break;
                    case "wood": rg.wood = qty[i]; break;
                }
            }
            return rg;
        }
    }
}
