using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace VoidCore
{
    /// <summary>
    /// Settings used to control the behaviour of VoidCore, such as turning on debug mode or enabling gameObject tracking
    /// </summary>
    public static class Settings
    {
        static bool started = false;

        [OnModStart(typeof(VoidCore))]
        static void SettingsStart()
        {
            if (DebugMode)
            {
                InternalGameObjectTracking = true;
            }
            if (InternalGameObjectTracking)
            {
                GameObjectTracker.StartTracker();
            }
            if (InternalDebugMode)
            {
                Events.InternalOnDebugStart?.Invoke();
            }
            started = true;
        }



        private static bool InternalGameObjectTracking = false;
        public static bool GameObjectTracking
        {
            get => InternalGameObjectTracking;
            set
            {
                if (started && value != InternalGameObjectTracking)
                {
                    InternalGameObjectTracking = value;
                    if (value)
                    {
                        GameObjectTracker.StartTracker();
                    }
                    else
                    {
                        DebugMode = false;
                        GameObjectTracker.StopTracker();
                    }
                }
            }
        }


        private static bool InternalDebugMode = false;
        public static bool DebugMode
        {
            get => InternalDebugMode;
            set
            {
                if (started && value != InternalDebugMode)
                {
                    InternalDebugMode = value;
                    if (value)
                    {
                        GameObjectTracking = true;
                        Events.InternalOnDebugStart?.Invoke();
                    }
                    else
                    {
                        Events.InternalOnDebugEnd?.Invoke();
                    }
                }
            }
        }
    }
}
