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
        /// Called whenever a gameObject is created in the game
        /// </summary>
        /// <remarks>
        /// Note that this could be called multiple times for a single gameobject
        /// </remarks>
        public static event Action<GameObject> GameObjectCreated
        {
            add => InternalGameObjectCreated += value;
            remove => InternalGameObjectCreated -= value;
        }
        internal static Action<GameObject> InternalGameObjectCreated;

        public static event Action OnDebugStart
        {
            add => InternalOnDebugStart += value;
            remove => InternalOnDebugStart -= value;
        }

        internal static Action InternalOnDebugStart;

        public static event Action OnDebugEnd
        {
            add => InternalOnDebugEnd += value;
            remove => InternalOnDebugEnd -= value;
        }
        internal static Action InternalOnDebugEnd;
    }
}
