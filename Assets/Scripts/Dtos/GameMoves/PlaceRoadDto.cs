using System;
using Assets.Scripts.Enums;

namespace Assets.Scripts.Dtos
{
    [Serializable]
    public class PlaceRoadDto
    {
        public int tileX;
        public int tileY;
        public int edgeIndex;

        public PlaceRoadDto(int tileX, int tileY, int edgeIndex)
        {
            this.tileX = tileX;
            this.tileY = tileY;
            this.edgeIndex = edgeIndex;
        }
    }
}
