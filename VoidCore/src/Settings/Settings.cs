using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using VoidCore.Hooks.Utility;
using Modding;
using VoidCore.Hooks.Internal;

namespace VoidCore
{

    /// <summary>
    /// Settings used to control the behaviour of VoidCore, such as turning on debug mode or enabling gameObject tracking
    /// </summary>
    public static class Settings
    {
        static bool started = false;

        internal class SettingsStart : IHook<VoidCore>
        {
            void IHookBase.LoadHook(IMod mod)
            {
                if (InternalGMTracking)
                {
                    Events.GMTrackingEvent.Invoker?.Invoke(true);
                }
                if (InternalDebugMode)
                {
                    Events.DebugEvent.Invoker?.Invoke(true);
                }
                started = true;
            }

            void IHookBase.UnloadHook(IMod mod)
            {
                started = false;
                if (InternalDebugMode)
                {
                    Events.DebugEvent.Invoker?.Invoke(false);
                }
                if (InternalGMTracking)
                {
                    Events.GMTrackingEvent.Invoker?.Invoke(false);
                }
            }
        }


        private static bool InternalGMTracking = false;
        /// <summary>
        /// Whether GameObject Tracking is enabled or not. If enabled, the <see cref="Events.GameObjectCreated"/> will also be enabled, but can cause the game to run slower, so use this only for debugging purposes
        /// </summary>
        /// <remarks>
        /// This must be enabled for <see cref="DebugMode"/> to work.
        /// </remarks>
        public static bool GMTracking
        {
            get => InternalGMTracking;
            set
            {
                if (started && value != InternalGMTracking)
                {
                    InternalGMTracking = value;
                    if (value)
                    {
                        //GameObjectTracker.StartTracker();
                        Events.GMTrackingEvent.Invoker?.Invoke(true);
                    }
                    else
                    {
                        DebugMode = false;
                        //GameObjectTracker.StopTracker();
                        Events.GMTrackingEvent.Invoker?.Invoke(false);
                    }
                }
            }
        }


        private static bool InternalDebugMode = false;
        /// <summary>
        /// Whether Debug Mode is enabled or not. If it's enabled, things such as collision box visualization will also be enabled
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
                        GMTracking = true;
                        Events.DebugEvent.Invoker?.Invoke(true);
                    }
                    else
                    {
                        Events.DebugEvent.Invoker?.Invoke(false);
                    }
                }
            }
        }
    }
}
