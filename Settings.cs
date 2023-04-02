using System;

namespace FakePeppino
{
    [Serializable]
    public class LocalSettings
    {
        public BossStatue.Completion Completion = new()
        {
            isUnlocked = true,
            hasBeenSeen = true,
        };
        public bool UsingAltVersion = false;
        public bool InBossDoor = false;
    }
}