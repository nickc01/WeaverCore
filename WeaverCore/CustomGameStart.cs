using System;
using System.Collections.Generic;
using System.Reflection;
using WeaverCore.Attributes;

namespace WeaverCore
{
    public static class CustomGameStart
    {
        [OnHarmonyPatch]
        static void OnHarmonyPatch(HarmonyPatcher patcher)
        {
            {
                var orig = typeof(GameManager).GetMethod("StartNewGame");
                var prefix = typeof(CustomGameStart).GetMethod(nameof(StartNewGame_Prefix), BindingFlags.NonPublic | BindingFlags.Static);
                var postfix = typeof(CustomGameStart).GetMethod(nameof(StartNewGame_Postfix), BindingFlags.NonPublic | BindingFlags.Static);
                patcher.Patch(orig, prefix, postfix);
            }

            {
                var orig = typeof(PlayerData).GetMethod(nameof(PlayerData.AddGGPlayerDataOverrides));
                var prefix = typeof(CustomGameStart).GetMethod(nameof(AddGGPlayerDataOverrides_Prefix), BindingFlags.NonPublic | BindingFlags.Static);
                patcher.Patch(orig, prefix, null);
            }
        }

        static bool cancelGGPlayerDataOverrides = false;

        static bool StartNewGame_Prefix(GameManager __instance, PlayerData ___playerData, ref bool permadeathMode, ref bool bossRushMode, ref bool __state)
        {
            WeaverLog.Log("CustomGameStart Calling Prefix!!!");
            bool continueGame = bossRushMode;
            foreach (var evt in gameStartDelegates)
            {
                try
                {
                    evt.Func?.Invoke(___playerData, ref permadeathMode, ref bossRushMode, ref continueGame);
                }
                catch(Exception e)
                {
                    WeaverLog.LogException(e);
                }
            }

            if (bossRushMode)
            {
                ___playerData.AddGGPlayerDataOverrides();
            }

            __state = continueGame;
            bossRushMode = continueGame;

            cancelGGPlayerDataOverrides = true;
            
            return true;
        }

        static void StartNewGame_Postfix()
        {
            cancelGGPlayerDataOverrides = false;
        }

        static bool AddGGPlayerDataOverrides_Prefix()
        {
            WeaverLog.Log("Running Player Data Overrides = " + !cancelGGPlayerDataOverrides);
            return !cancelGGPlayerDataOverrides;
        }

        class Event : IComparable<Event>
        {
            public OnGameStartDelegate Func;
            public long Priority;

            public int CompareTo(Event other)
            {
                return Priority.CompareTo(other.Priority);
            }
        }

        public delegate void OnGameStartDelegate(PlayerData playerData, ref bool permaDeathMode, ref bool bossRushMode, ref bool continueGame);

        static SortedSet<Event> gameStartDelegates = new SortedSet<Event>();

        public static bool Add(OnGameStartDelegate func, long priority = 0)
        {
            if (func == null)
                return false;

            var evt = new Event { Func = func, Priority = priority };
            return gameStartDelegates.Add(evt);
        }

        public static bool Remove(OnGameStartDelegate func)
        {
            if (func == null)
                return false;

            Event toRemove = null;
            foreach (var evt in gameStartDelegates)
            {
                if (evt.Func == func)
                {
                    toRemove = evt;
                    break;
                }
            }

            if (toRemove != null)
            {
                return gameStartDelegates.Remove(toRemove);
            }

            return false;
        }

        /*public static void InvokeAll()
        {
            foreach (var evt in gameStartDelegates)
            {
                evt.Func?.Invoke();
            }
        }

        public static void Clear()
        {
            gameStartDelegates.Clear();
        }*/
    }
}
