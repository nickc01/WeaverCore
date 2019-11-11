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

        [ModStart(typeof(VoidCore))]
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
        /// <summary>
        /// Whether GameObject Tracking is enabled or not. If enabled, the <see cref="Events.GameObjectCreated"/> will also be enabled
        /// </summary>
        /// <remarks>
        /// This must be enabled for <see cref="DebugMode"/> to work.
        /// </remarks>
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
        /// <summary>
        /// Whether DebugMode is enabled or not. If it's enabled, things such as collision box visualization will also be enabled
        /// </summary>
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
