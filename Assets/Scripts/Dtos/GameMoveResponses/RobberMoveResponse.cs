namespace Assets.Scripts.Dtos.GameMoveResponses
{
    public class RobberMoveResponse
    {
        public int originatingTileX;
        public int originatingTileY;
        public int destinationTileX;
        public int destinationTileY;
        public string username;

        public RobberMoveResponse(int originatingTileX, int originatingTileY, int destinationTileX, int destinationTileY, string username)
        {
            this.originatingTileX = originatingTileX;
            this.originatingTileY = originatingTileY;
            this.destinationTileX = destinationTileX;
            this.destinationTileY = destinationTileY;
            this.username = username;
        }
    }
}
