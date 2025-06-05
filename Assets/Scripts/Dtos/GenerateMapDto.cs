using Assets.Scripts.Dtos.Board;
using System.Collections.Generic;

namespace Assets.Scripts.Dtos
{
    public class GenerateMapDto
    {
        public BoardStateDto board { get; set; }

        public GenerateMapDto(BoardStateDto board)
        {
            this.board = board;
        }
    }
}
