using System;

namespace Assets.Scripts.Models
{
    [Serializable]
    public class DevCardDto
    {
        public long id;
        public DevCardType type;
        public bool playable;
        public bool used;

        public DevCardDto() { }

        public DevCardDto(long id, DevCardType type, bool playable, bool used)
        {
            this.id = id;
            this.type = type;
            this.playable = playable;
            this.used = used;
        }
    }
}
