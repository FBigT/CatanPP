using System;
using Assets.Scripts.GameMode.Trading.Models;
using Assets.Scripts.Models;

namespace Assets.Scripts.Dtos.GameMoves
{
    [Serializable]
    public class PlayCardDto
    {
        public PlayCardDto(DevCardType devCardType) {
            if (devCardType != DevCardType.VICTORY_POINT) {
                throw new ArgumentException("Additional information needed");
            }
            DevCardType = devCardType;
        }

        public PlayCardDto(PlaceRoadDto road1, PlaceRoadDto road2) { 
            DevCardType = DevCardType.ROAD_BUILDING;
            moveData = new PlaceRoadDto[2] { road1, road2 };
        }

        public PlayCardDto(RobberMoveDto robberMoveDto) { 
            DevCardType = DevCardType.KNIGHT;
            moveData = robberMoveDto;
        }

        public PlayCardDto(ResourceGroup resourceGroup) {
            DevCardType = DevCardType.YEAR_OF_PLENTY;
            moveData = resourceGroup;
        }

        public DevCardType DevCardType { get; set; }
        public object moveData;
    }
}
