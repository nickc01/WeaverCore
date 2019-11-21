using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VoidCore.Helpers;

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
        public static InternalEvent<Action<GameObject, bool>> GameObjectCreated;

        /// <summary>
        /// Called whenever a gameobject is either destroyed or disabled. If the boolean is true, then the gameObject is being destroyed. If it's false, then that gameObject is being disabled
        /// </summary>
        public static InternalEvent<Action<GameObject,bool>> GameObjectRemoved;

        public static InternalEvent<Action<bool>> DebugEvent;

        public static InternalEvent<Action<bool>> GMTrackingEvent;
    }
}
