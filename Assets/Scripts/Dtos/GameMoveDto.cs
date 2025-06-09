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
            if (gameMoveType != GameMoveType.DICE_ROLL &&
                gameMoveType != GameMoveType.END_TURN &&
                gameMoveType != GameMoveType.BUY_CARD &&
                gameMoveType != GameMoveType.TURN_ORDER &&
                gameMoveType != GameMoveType.REQUEST_MAP &&
                gameMoveType != GameMoveType.REQUEST_DEV_CARDS)
                throw new ArgumentException("Passed game move type requires additional data");
            this.gameMoveType = gameMoveType;
        }

        public GameMoveDto(RobberMoveDto robberMoveDto)
        {
            gameMoveType = GameMoveType.ROBBER_MOVE;
            moveData = robberMoveDto;
        }

        public GameMoveDto(UpgradeStructureDto upgradeStructureDto)
        {
            gameMoveType = GameMoveType.UPGRADE_STRUCTURE;
            moveData = upgradeStructureDto;
        }

        public GameMoveDto(PlaceRoadDto placeRoadDto)
        {
            gameMoveType = GameMoveType.PLACE_ROAD;
            moveData = placeRoadDto;
        }

        public GameMoveDto(PlaceStructureDto placeStructureDto)
        {
            gameMoveType = GameMoveType.PLACE_STRUCTURE;
            moveData = placeStructureDto;
        }

        public GameMoveDto(PlayCardDto playCardDto)
        {
            gameMoveType = GameMoveType.PLAY_CARD;
            moveData = playCardDto;
        }

        public GameMoveDto(TradeResponseMessage resp)
        {
            gameMoveType = GameMoveType.TRADE_RESPONSE;
            moveData = resp;
        }

        public GameMoveDto(TradeOfferMessage tradeOffer)
        {
            gameMoveType = GameMoveType.TRADE_OFFER;
            moveData = tradeOffer;
        }

        public GameMoveDto(GenerateMapDto generateMapDto)
        {
            gameMoveType = GameMoveType.MAP_GEN;
            moveData = generateMapDto;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public GameMoveType gameMoveType;
        public object moveData;
    }
}
