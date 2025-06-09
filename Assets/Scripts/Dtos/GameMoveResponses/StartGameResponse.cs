using System.Collections.Generic;

namespace Assets.Scripts.Dtos.GameMoveResponses
{
    public class StartGameResponse
    {
        public List<TileDto> tiles;
        public List<string> turnOrder;
    }
}
