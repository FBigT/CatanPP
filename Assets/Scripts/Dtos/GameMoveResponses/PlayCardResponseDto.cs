using System;
using Assets.Scripts.Models;
using Assets.Scripts.Utils;
using Newtonsoft.Json;

namespace Assets.Scripts.Dtos.GameMoveResponses
{
    [Serializable]
    [JsonConverter(typeof(PlayCardResponseConverter))]
    public class PlayCardResponseDto
    {
        public DevCardType devCardType;
        public object moveData;
    }
}
