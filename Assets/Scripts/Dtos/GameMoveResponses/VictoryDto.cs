using System;
using System.Collections.Generic;

namespace Assets.Scripts.Dtos.GameMoveResponses
{
    [Serializable]
    public class VictoryDto
    {
        public List<PlayerScoreDto> players;
    }
}
