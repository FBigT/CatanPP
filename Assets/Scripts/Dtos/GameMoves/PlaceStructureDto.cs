using System;
using Assets.Scripts.Enums;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Assets.Scripts.Dtos
{
    [Serializable]
    public class PlaceStructureDto
    {
        public int tileX;
        public int tileY;
        public int cornerIndex;
        [JsonConverter(typeof(StringEnumConverter))]
        public StructureType structureType;

        public PlaceStructureDto(int tileX, int tileY, int cornerIndex, StructureType structureType)
        {
            this.tileX = tileX;
            this.tileY = tileY;
            this.cornerIndex = cornerIndex;
            this.structureType = structureType;
        }
    }
}
