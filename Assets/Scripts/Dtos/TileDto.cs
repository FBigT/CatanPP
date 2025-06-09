using System;

namespace Assets.Scripts.Dtos
{
    [Serializable]
    public class TileDto
    {
        public int x;
        public int y;
        public int z=0;
        public string tileType;
        public int number;

        public TileDto(int x, int y, string tileType, int number,int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.tileType = tileType;
            this.number = number;
        }
    }
}
