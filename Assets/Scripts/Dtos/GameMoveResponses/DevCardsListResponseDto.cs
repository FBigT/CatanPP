using System;
using System.Collections.Generic;
using Assets.Scripts.Models;

namespace Assets.Scripts.Dtos.GameMoveResponses
{
    [Serializable]
    public class DevCardsListResponseDto
    {
        public List<DevCardDto> devCards;
        public string username;
    }
}
