using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.GameMode.Trading.Models
{
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
        public int brick, crystal, ore, rice, sheep, silver, gold, wood;
    }
}
