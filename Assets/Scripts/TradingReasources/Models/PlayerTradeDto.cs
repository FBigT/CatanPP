using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.TradingReasources.Models
{

    [Serializable]
    public class PlayerTradeDto
    {
        public long sessionId;
        public string fromUser;
        public string toUser;
        public ResourceGroup offered;
        public ResourceGroup requested;
    }
}
