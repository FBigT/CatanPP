using System;

namespace Assets.Scripts.Dtos
{
    [Serializable]
    public class TileDto
    {
        public int x;
        public int y;
        public string tileType;
        public int number;

        public TileDto(int x, int y, string tileType, int number)
        {
            this.x = x;
            this.y = y;
            this.tileType = tileType;
            this.number = number;
        }
    }
}
