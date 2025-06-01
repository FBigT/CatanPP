using System;
using Assets.Scripts.Dtos.GameMoveResponses;
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
            if (gameMoveType != GameMoveType.DICE_ROLL && gameMoveType != GameMoveType.END_TURN && gameMoveType != GameMoveType.BUY_CARD && gameMoveType != GameMoveType.TURN_ORDER)
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


        public GameMoveDto(PlaceStructureDto placeStructureDto){
            GameMoveType = GameMoveType.PLACE_STRUCTURE;
            moveData = placeStructureDto;
        }

        public GameMoveDto(PlayCardDto playCardDto)
        {
            GameMoveType = GameMoveType.PLAY_CARD;
            moveData = playCardDto;
        }
        public GameMoveDto(TradeResponseMessage resp)
        {
            GameMoveType = GameMoveType.TRADE_RESPONSE;
            moveData = resp;
        }


        public GameMoveDto(TradeOfferMessage tradeOffer)
        {
            GameMoveType = GameMoveType.TRADE_OFFER;
            moveData = tradeOffer;
        }
        [JsonConverter(typeof(StringEnumConverter))]
        public GameMoveType GameMoveType { get; set; }
        public object moveData;
    }
}
