using System.Collections.Generic;

namespace Assets.Scripts.Dtos
{
    public class GenerateMapDto
    {
        public List<TileDto> tileDtos;

        public GenerateMapDto(List<TileDto> tileDtos)
        {
            this.tileDtos = tileDtos;
        }
    }
}
