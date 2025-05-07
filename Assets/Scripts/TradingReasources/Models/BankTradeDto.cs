using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.TradingReasources.Models
{
    [Serializable]
    public class BankTradeDto
    {
        public long sessionId;
        public string fromUser;
        public ResourceGroup offered;
        public ResourceGroup requested;
        public string portType;
        public int portRatio;
    }
}
