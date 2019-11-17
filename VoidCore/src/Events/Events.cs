using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VoidCore
{
    /// <summary>
    /// A list of events provided by VoidCore
    /// </summary>
    public static class Events
    {
        /// <summary>
        /// Called whenever a gameobject is either created or enabled. If the boolean is true, then the gameObject is being created. If it's false, then that gameObject is being enabled
        /// </summary>
        public static event Action<GameObject,bool> GameObjectCreated
        {
            add => InternalGameObjectCreated += value;
            remove => InternalGameObjectCreated -= value;
        }
        internal static Action<GameObject,bool> InternalGameObjectCreated;

        /// <summary>
        /// Called whenever a gameobject is either destroyed or disabled. If the boolean is true, then the gameObject is being destroyed. If it's false, then that gameObject is being disabled
        /// </summary>
        public static event Action<GameObject,bool> GameObjectRemoved
        {
            add => InternalGameObjectRemoved += value;
            remove => InternalGameObjectRemoved -= value;
        }
        internal static Action<GameObject,bool> InternalGameObjectRemoved;

        public static event Action<bool> DebugEvent
        {
            add => InternalDebugEvent += value;
            remove => InternalDebugEvent -= value;
        }
        internal static Action<bool> InternalDebugEvent;

        public static event Action<bool> GMTrackingEvent
        {
            add => InternalGMTrackingEvent += value;
            remove => InternalGMTrackingEvent -= value;
        }
        internal static Action<bool> InternalGMTrackingEvent;
    }
}
