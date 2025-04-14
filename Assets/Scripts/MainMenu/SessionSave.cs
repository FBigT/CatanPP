using System;

namespace Assets.Scripts.MainMenu
{
    public class SessionSave
    {
        public SessionSave(/*long id,*/ string saveName, int turnNumber, DateTime dateTime)
        {
            Id = 0;
            SaveName = saveName;
            TurnNumber = turnNumber;
            DateTime = dateTime;
        }

        public long Id { get; set; }
        public string SaveName { get; set; }
        public int TurnNumber { get; set; }
        public DateTime DateTime { get; set; }
    }
}
