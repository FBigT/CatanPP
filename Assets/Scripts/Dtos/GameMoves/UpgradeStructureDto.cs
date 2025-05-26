using System;

namespace Assets.Scripts.Dtos
{
    [Serializable]
    public class UpgradeStructureDto
    {
        public int tileX;
        public int tileY;
        public int cornerIndex;

        public UpgradeStructureDto(int tileX, int tileY, int cornerIndex)
        {
            this.tileX = tileX;
            this.tileY = tileY;
            this.cornerIndex = cornerIndex;
        }
    }
}
