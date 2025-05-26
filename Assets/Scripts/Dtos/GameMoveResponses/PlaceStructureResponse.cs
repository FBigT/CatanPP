using Assets.Scripts.Enums;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Assets.Scripts.Dtos.GameMoveResponses
{
    public class PlaceStructureResponse
    {
        public int tileX;
        public int tileY;
        public int cornerIndex;
        [JsonConverter(typeof(StringEnumConverter))]
        public StructureType structureType;
        public string username;

        public PlaceStructureResponse(int tileX, int tileY, int cornerIndex, StructureType structureType, string username)
        {
            this.tileX = tileX;
            this.tileY = tileY;
            this.cornerIndex = cornerIndex;
            this.structureType = structureType;
            this.username = username;
        }
    }
}
