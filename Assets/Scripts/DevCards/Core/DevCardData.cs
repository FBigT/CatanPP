using UnityEngine;

namespace CatanGame.DevCards.Core
{
    [System.Serializable]
    public class DevCardData
    {
        [Header("Card Info")]
        public long id;
        public DevCardType type;
        public bool playable;
        public bool used;

        [Header("Display")]
        public string title;
        public string description;
        public Sprite icon;

        public DevCardData() { }

        public DevCardData(long id, DevCardType type, bool playable, bool used)
        {
            this.id = id;
            this.type = type;
            this.playable = playable;
            this.used = used;

            // Set display info based on type
            SetDisplayInfo();
        }

        private void SetDisplayInfo()
        {
            switch (type)
            {
                case DevCardType.KNIGHT:
                    title = "Knight";
                    description = "Move robber and steal from player";
                    break;
                case DevCardType.VICTORY_POINT:
                    title = "Victory Point";
                    description = "Instant victory point";
                    break;
                case DevCardType.ROAD_BUILDING:
                    title = "Road Building";
                    description = "Place 2 free roads";
                    break;
                case DevCardType.YEAR_OF_PLENTY:
                    title = "Year of Plenty";
                    description = "Choose 2 free resources";
                    break;
            }
        }

        public bool CanPlay()
        {
            return playable && !used;
        }
    }
}
