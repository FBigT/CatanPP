namespace Assets.Scripts.Dtos.GameMoveResponses
{
    public class UpgradeStructureResponse
    {
        public int tileX;
        public int tileY;
        public int cornerIndex;
        public string username;

        public UpgradeStructureResponse(int tileX, int tileY, int cornerIndex, string username)
        {
            this.tileX = tileX;
            this.tileY = tileY;
            this.cornerIndex = cornerIndex;
            this.username = username;
        }
    }
}
