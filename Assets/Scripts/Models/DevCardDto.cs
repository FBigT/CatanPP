using System;
namespace Assets.Scripts.Models {
    // Must match backend enum names exactly:
    public enum DevCardType
    {
        KNIGHT,
        VICTORY_POINT,
        ROAD_BUILDING,
        YEAR_OF_PLENTY
    }

    [Serializable]
    public class DevCardDto
    {
        public long id;
        public DevCardType type;
        public bool playable;
        public bool used;
    }
}

