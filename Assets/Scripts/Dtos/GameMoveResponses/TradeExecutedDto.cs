using Assets.Scripts.GameMode.Trading.Models;
using System;

namespace Assets.Scripts.Dtos.GameMoveResponses
{
    [Serializable]
    public class TradeExecutedDto
    {
        public string fromUser;
        public string toUser;
        public ResourceGroup offered;
        public ResourceGroup requested;
    }
}
