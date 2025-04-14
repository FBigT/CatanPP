using System;

namespace Assets.Scripts.User
{
    [Serializable]
    public class PlayerProfile
    {
        public string username;
        public string gamesWon;
        public string gamesPlayed;
        public string gamesLost;
        public string turnsTaken;
        public string resourcesGathered;
        public string structuresPlaced;
        public string roadsPlaced;
        public string skinsUnlocked;
    }
}
