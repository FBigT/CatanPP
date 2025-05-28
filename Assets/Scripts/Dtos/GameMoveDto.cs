using System;
using Assets.Scripts.Dtos.GameMoves;
using Assets.Scripts.Enums;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Assets.Scripts.Dtos
{
    [Serializable]
    public class GameMoveDto
    {
        public GameMoveDto() { }

        public GameMoveDto(GameMoveType gameMoveType)
        {
            if (gameMoveType == GameMoveType.VICTORY)
                throw new ArgumentException("You do not decide this");
            if (gameMoveType != GameMoveType.DICE_ROLL && gameMoveType != GameMoveType.END_TURN && gameMoveType != GameMoveType.BUY_CARD)
                throw new ArgumentException("Passed game move type requires additional data");
            GameMoveType = gameMoveType;
        }

        public GameMoveDto(RobberMoveDto robberMoveDto)
        {
            GameMoveType = GameMoveType.ROBBER_MOVE;
            moveData = robberMoveDto;
        }

        public GameMoveDto(UpgradeStructureDto upgradeStructureDto)
        {
            GameMoveType = GameMoveType.UPGRADE_STRUCTURE;
            moveData = upgradeStructureDto;
        }

        public GameMoveDto(PlaceRoadDto placeRoadDto)
        {
            GameMoveType = GameMoveType.PLACE_ROAD;
            moveData = placeRoadDto;
        }
        // Add to existing GameMoveDto.cs
        // Add this to your existing GameMoveDto.cs file, around line 37
        //public GameMoveDto(Assets.Scripts.Dtos.GameMoves.PlayCardDto playCardDto)
        //{
        //    GameMoveType = GameMoveType.PLAY_CARD;
        //    moveData = playCardDto;
        //}


        public GameMoveDto(PlaceStructureDto placeStructureDto)
        {
            GameMoveType = GameMoveType.PLACE_STRUCTURE;
            moveData = placeStructureDto;
        }

        public GameMoveDto(PlayCardDto playCardDto)
        {
            GameMoveType = GameMoveType.PLAY_CARD;
            moveData = playCardDto;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public GameMoveType GameMoveType { get; set; }
        public object moveData;
    }
}
