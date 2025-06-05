using System.Collections.Generic;
using Assets.Scripts.Enums;

namespace Gamemode.New
{
    public class ResourceGroup
    {
        public Dictionary<ResourceType, int> Resources = new();

        public ResourceGroup()
        {
            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                Resources[type] = 0;
            }
        }

        public void Add(ResourceType type, int amount)
        {
            Resources[type] += amount;
        }

        public bool HasEnough(ResourceGroup cost)
        {
            foreach (var kvp in cost.Resources)
            {
                if (Resources[kvp.Key] < kvp.Value)
                    return false;
            }
            return true;
        }

        public void Subtract(ResourceGroup cost)
        {
            foreach (var kvp in cost.Resources)
            {
                Resources[kvp.Key] -= kvp.Value;
            }
        }
    }
}

