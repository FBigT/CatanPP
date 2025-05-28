using System;
using Assets.Scripts.GameMode.Trading.Models;

namespace Assets.Scripts.Dtos.GameMoves
{
    [Serializable]
    public class TradeOfferDto
    {
        public string fromUser;
        public string toUser;
        public ResourceGroup offered;
        public ResourceGroup requested;
    }
}
