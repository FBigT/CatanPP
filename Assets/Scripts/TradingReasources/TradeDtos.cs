using System;

namespace Assets.Scripts.TradingReasources
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
    }

    [Serializable]
    public class PlayerTradeDto
    {
        public long sessionId;
        public string fromUser;
        public string toUser;
        public ResourceGroup offered;
        public ResourceGroup requested;
    }

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

    [Serializable]
    public class SessionPlayerDto
    {
        public long id;
        public long sessionId;
        public long userId;
        public string username;
        public int playerScore;
        public bool active;
        public bool isAi;
        public string name;
        public int brick;
        public int crystal;
        public int ore;
        public int rice;
        public int sheep;
        public int silver;
        public int gold;
        public int wood;
    }
}
