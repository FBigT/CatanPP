using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Assets.Scripts.Dtos
{
    [Serializable]
    public class JoinSessionNotification
    {
        public List<string> usernames;
    }
}
