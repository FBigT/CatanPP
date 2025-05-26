using System;

namespace Assets.Scripts.Dtos.GameMoveResponses
{
    [Serializable]
    public class EndTurnResponse
    {
        public string previousPlayerName;
        public string currentPlayerName;
        public string nextPlayerName;
        public int turnNumber;
    }
}
