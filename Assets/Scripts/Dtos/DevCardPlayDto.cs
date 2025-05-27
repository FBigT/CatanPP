using System.Collections.Generic;

namespace CatanGame.DTOs
{
    [System.Serializable]
    public class DevCardPlayDto
    {
        public long id;
        public Dictionary<string, object> cardPlayData;

        public DevCardPlayDto()
        {
            cardPlayData = new Dictionary<string, object>();
        }

        public DevCardPlayDto(long cardId)
        {
            id = cardId;
            cardPlayData = new Dictionary<string, object>();
        }

        public DevCardPlayDto(long cardId, Dictionary<string, object> data)
        {
            id = cardId;
            cardPlayData = data ?? new Dictionary<string, object>();
        }
    }
}
