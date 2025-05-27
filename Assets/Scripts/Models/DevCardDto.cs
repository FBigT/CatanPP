using CatanGame.DevCards.Core;

namespace CatanGame.DTOs
{
    [System.Serializable]
    public class DevCardDto
    {
        public long id;
        public string type; // "KNIGHT", "VICTORY_POINT", etc.
        public bool playable;
        public bool used;

        public DevCardData ToDevCardData()
        {
            DevCardType cardType;
            System.Enum.TryParse(type, out cardType);

            return new DevCardData(id, cardType, playable, used);
        }
    }
}
