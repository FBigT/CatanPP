namespace Assets.Scripts.Dtos.GameMoveResponses
{
    public class RobberMoveResponse
    {
        public int originatingTileX;
        public int originatingTileY;
        public int destinationTileX;
        public int destinationTileY;
        public string moverName;

        public string victimName;
        public string resourceName;

        public RobberMoveResponse(int originatingTileX, int originatingTileY, int destinationTileX, int destinationTileY, string moverName, string victimName, string resourceName)
        {
            this.originatingTileX = originatingTileX;
            this.originatingTileY = originatingTileY;
            this.destinationTileX = destinationTileX;
            this.destinationTileY = destinationTileY;
            this.moverName = moverName;
            this.victimName = victimName;
            this.resourceName = resourceName;
        }
    }
}
