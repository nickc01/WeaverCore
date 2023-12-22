using System;
using UnityEngine;

namespace WeaverCore.Internal
{
    public static class Other_Preloads
    {
        //public static GameObject JournalUpdateMsg;
        public static GameObject HealthCocoonFlashPrefab;
        public static AudioSource AudioPlayerPrefab;

        public static GameObject smallGeoPrefab;
        public static GameObject mediumGeoPrefab;
        public static GameObject largeGeoPrefab;

        public static GameObject JournalUpdateMessagePrefab;

        public static Func<GameObject> GetJournalUpdateMessageSpawnedFunc;
        public static Action<GameObject> SetJournalUpdateMessageSpawnedFunc;
    }
}