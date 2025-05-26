using Assets.Scripts.Enums;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Assets.Scripts.Dtos.GameMoveResponses;
using Assets.Scripts.Utils;
using System;

namespace Assets.Scripts.Dtos
{
    [Serializable]
    [JsonConverter(typeof(GameMoveResponseConverter))]
    public class GameMoveResponseDto
    {
        public GameMoveResponseDto() { }

        public GameMoveResponseDto(EndTurnResponse endTurnResponse) { 
            GameMoveType = GameMoveType.END_TURN;
            moveData = endTurnResponse;
        }

        public GameMoveResponseDto(DiceResultDto diceResultDto) { 
            GameMoveType = GameMoveType.DICE_ROLL;
            moveData = diceResultDto;
        }

        public GameMoveResponseDto(RobberMoveResponse robberMoveDto)
        {
            GameMoveType = GameMoveType.ROBBER_MOVE;
            moveData = robberMoveDto;
        }

        public GameMoveResponseDto(UpgradeStructureResponse upgradeStructureDto)
        {
            GameMoveType = GameMoveType.UPGRADE_STRUCTURE;
            moveData = upgradeStructureDto;
        }

        public GameMoveResponseDto(PlaceRoadResponse placeRoadDto)
        {
            GameMoveType = GameMoveType.PLACE_ROAD;
            moveData = placeRoadDto;
        }

        public GameMoveResponseDto(PlaceStructureResponse placeStructureDto)
        {
            GameMoveType = GameMoveType.PLACE_STRUCTURE;
            moveData = placeStructureDto;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public GameMoveType GameMoveType { get; set; }
        public object moveData;
    }
}
