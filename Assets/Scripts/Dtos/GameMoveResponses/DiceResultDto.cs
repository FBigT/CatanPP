using System.Collections.Generic;
using Assets.Scripts.GameMode.Trading.Models;

namespace Assets.Scripts.Dtos
{
    public class DiceResultDto
    {
        public string username;
        public int rollResult;
        public Dictionary<string, ResourceGroup> userResourcesGained;
    }
}
