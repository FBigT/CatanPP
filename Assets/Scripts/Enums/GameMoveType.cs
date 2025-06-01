// Assets/Scripts/UI/Test/GameMoveType.cs
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Assets.Scripts.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GameMoveType
    {
        PLACE_ROAD,
        PLACE_STRUCTURE,
        BUY_CARD,
        PRIVATE_BUY_CARD,
        UPGRADE_STRUCTURE,
        END_TURN,
        DICE_ROLL,
        ROBBER_MOVE,
        PLAY_CARD,
        VICTORY,
        MAP_GEN,
        TRADE_OFFER,
        TRADE_RESPONSE,
        TURN_ORDER
    }
}
