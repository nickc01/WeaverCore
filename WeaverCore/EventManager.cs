using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;
using WeaverCore.Internal;
using WeaverCore.Utilities;

namespace WeaverCore
{
    /// <summary>
    /// Used for receiving events from other objects, and sending events to other objects. This component can also be used to send and receive PlaymakerFSM events
    /// </summary>
    public sealed class EventManager : MonoBehaviour
    {
        /// <summary>
        /// The type of event that is getting sent
        /// </summary>
        public enum EventType
        {
            /// <summary>
            /// A message that is being sent to only one object
            /// </summary>
            Message,

            /// <summary>
            /// A broadcast that is being sent to all objects in the scene
            /// </summary>
            Broadcast
        }

        EventManager_I impl;
        static EventManager_I.Statics implStatics = ImplFinder.GetImplementation<EventManager_I.Statics>();

        static HashSet<EventManager> allReceivers = new HashSet<EventManager>();

        /// <summary>
        /// A delegate used for receiving events
        /// </summary>
        /// <param name="eventName">The name of the event received</param>
        /// <param name="source">The source object the event came from</param>
        public delegate void EventReceiveDelegate(string eventName, GameObject source);

        /// <summary>
        /// A delegate used whenever an event gets triggered
        /// </summary>
        /// <param name="eventName">The name of the event received</param>
        /// <param name="source">The source object the event came from</param>
        /// <param name="destination">The destination object the event is being set to. Will be null if no destination was specified</param>
        public delegate void EventTriggeredDelegate(string eventName, GameObject source, GameObject destination, EventType eventType);

        /// <summary>
        /// Called whenever this gameObject receives an event
        /// </summary>
        public event EventReceiveDelegate OnReceivedEvent;

        /// <summary>
        /// Called anytime an event is triggered anywhere
        /// </summary>
        public static event EventTriggeredDelegate OnEventTriggered;

        List<(string eventName, Action<string, GameObject> source)> eventSpecificHooks = new List<(string eventName, Action<string, GameObject> source)>();

        /// <summary>
        /// Executes an action whenever an event with the specified <paramref name="name"/> is received
        /// </summary>
        /// <param name="name">The event name</param>
        /// <param name="action">The action to execute when the event of the specified name is received</param>
        public void AddReceiverForEvent(string name, Action action)
        {
            eventSpecificHooks.Add((name, (n, g) => action()));
        }

        /// <summary>
        /// Executes an action whenever an event with the specified <paramref name="name"/> is received
        /// </summary>
        /// <param name="name">The event name</param>
        /// <param name="action">The action to execute when the event of the specified name is received. The gameObject parameter is the source gameObject</param>
        public void AddReceiverForEvent(string name, Action<GameObject> action)
        {
            eventSpecificHooks.Add((name, (n, g) => action(g)));
        }

        /// <summary>
        /// Executes an action whenever an event with the specified <paramref name="name"/> is received
        /// </summary>
        /// <param name="name">The event name</param>
        /// <param name="action">The action to execute when the event of the specified name is received. The string parameter is the name of the event received, and the gameObject parameter is the source gameObject</param>
        public void AddReceiverForEvent(string name, Action<string, GameObject> action)
        {
            eventSpecificHooks.Add((name, action));
        }

        /// <summary>
        /// Clears all receivers for a particular event name
        /// </summary>
        /// <param name="name"></param>
        public void ClearReceiversForEvent(string eventName)
        {
            eventSpecificHooks.RemoveAll(pair => pair.eventName == eventName);
        }

        /// <summary>
        /// Clears all event receivers
        /// </summary>
        public void ClearAllReceivers()
        {
            eventSpecificHooks.Clear();
        }

        /// <summary>
        /// Fires an event on this object
        /// </summary>
        /// <param name="eventName">The event to fire</param>
        /// <param name="source">The object that is sending the event</param>
        public void TriggerEvent(string eventName, GameObject source)
        {
            TriggerEventInternal(eventName, source);
            RegisterTriggeredEvent(eventName, source, gameObject, EventType.Message);
            implStatics.TriggerEventToGameObjectPlaymakerFSMs(eventName, gameObject, source, true);
        }

        /// <summary>
        /// Broadcasts an event to all objects in the game
        /// </summary>
        /// <param name="eventName">The event to fire</param>
        /// <param name="source">The object sending the event</param>
        public static void BroadcastEvent(string eventName, GameObject source)
        {
            BroadcastEventInternal(eventName, source);
            RegisterTriggeredEvent(eventName, source, null, EventType.Broadcast);
            implStatics.BroadcastToPlaymakerFSMs(eventName, source, true);
        }

        /// <summary>
        /// Sends an event to a specific gameObject
        /// </summary>
        /// <param name="eventName">The event to fire</param>
        /// <param name="destination">The object receiving the event</param>
        /// <param name="source">The object sending the event</param>
        public static void SendEventToGameObject(string eventName, GameObject destination, GameObject source = null)
        {
            if (destination == null)
            {
                return;
            }
            foreach (var receiver in destination.GetComponents<EventManager>())
            {
                receiver.TriggerEventInternal(eventName, source);
            }
            implStatics.TriggerEventToGameObjectPlaymakerFSMs(eventName, destination, source, true);
        }

        /// <summary>
        /// Sends an event to a specific gameObject
        /// </summary>
        /// <param name="eventName">The event to fire</param>
        /// <param name="destination">The object receiving the event</param>
        /// <param name="delay">The delay before the event gets sent</param>
        /// <param name="source">The object sending the event</param>
        public static void SendEventToGameObject(string eventName, GameObject destination, float delay, GameObject source = null)
        {
            if (destination == null)
            {
                return;
            }

            IEnumerator Delay(float delay)
            {
                yield return new WaitForSeconds(delay);

                if (!Application.isPlaying || destination == null)
                {
                    yield break;
                }

                foreach (var receiver in destination.GetComponents<EventManager>())
                {
                    receiver.TriggerEventInternal(eventName, source);
                }
                implStatics.TriggerEventToGameObjectPlaymakerFSMs(eventName, destination, source, true);
            }

            UnboundCoroutine.Start(Delay(delay));
        }

        internal void TriggerEventInternal(string eventName, GameObject source)
        {
            OnReceivedEvent?.Invoke(eventName, source);
            foreach (var pair in eventSpecificHooks)
            {
                if (pair.eventName == eventName)
                {
                    pair.source?.Invoke(eventName, source);
                }
            }
        }

        internal static void BroadcastEventInternal(string eventName, GameObject source)
        {
            foreach (var receiver in allReceivers)
            {
                receiver.OnReceivedEvent?.Invoke(eventName, source);
                foreach (var pair in receiver.eventSpecificHooks)
                {
                    if (pair.eventName == eventName)
                    {
                        pair.source?.Invoke(eventName, source);
                    }
                }
            }
        }

        internal static void RegisterTriggeredEvent(string eventName, GameObject source, GameObject destination, EventType eventType)
        {
            OnEventTriggered?.Invoke(eventName, source, destination, eventType);
        }

        private void Awake()
        {
            var implType = ImplFinder.GetImplementationType<EventManager_I>();
            impl = (EventManager_I)GetComponent(implType);
            if (impl == null)
            {
                impl = (EventManager_I)gameObject.AddComponent(implType);
            }
            allReceivers.Add(this);
        }

        private void OnEnable()
        {
            allReceivers.Add(this);
        }

        private void OnDisable()
        {
            allReceivers.Remove(this);
        }

        private void OnDestroy()
        {
            allReceivers.Remove(this);
        }
    }
}
