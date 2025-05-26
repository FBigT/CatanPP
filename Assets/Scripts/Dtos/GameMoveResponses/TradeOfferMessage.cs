using System;
using Assets.Scripts.GameMode.Trading.Models;

namespace Assets.Scripts.Dtos.GameMoveResponses
{
    [Serializable]
    public class TradeOfferMessage
    {
        public string fromUser;
        public string toUser;
        public ResourceGroup offered;
        public ResourceGroup requested;
    }
}
