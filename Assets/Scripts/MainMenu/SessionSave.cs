using System;

namespace Assets.Scripts.MainMenu
{
    public class SessionSave
    {
        public SessionSave(string saveName, int turnNumber, DateTime dateTime)
        {
            SaveName = saveName;
            TurnNumber = turnNumber;
            DateTime = dateTime;
        }

        public string SaveName { get; set; }
        public int TurnNumber { get; set; }
        public DateTime DateTime { get; set; }
    }
}
