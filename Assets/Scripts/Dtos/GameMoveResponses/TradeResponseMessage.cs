using Assets.Scripts.GameMode.Trading.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Dtos.GameMoveResponses
{
    [Serializable]

    public class TradeResponseMessage
    {
        public string fromUser;   // Bob
        public string toUser;     // Alice
        public bool accepted;
        public long sessionId;
        public ResourceGroup offered;    // copy of original offer
        public ResourceGroup requested;
    }
}
