using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Assets.Scripts.Dtos.GameMoveResponses
{
    [Serializable]
    public class TurnOrderResponse
    {
        public List<string> usernames;
    }
}
