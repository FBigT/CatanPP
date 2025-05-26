using System;
using Assets.Scripts.Models;

namespace Assets.Scripts.Dtos.GameMoveResponses
{
    [Serializable]
    public class PrivateBuyCard
    {
        public DevCardType devCardType;
        public long cardId;
    }
}
