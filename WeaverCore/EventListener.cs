using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeaverCore
{
    /// <summary>
    /// Used for listening in on any events sent from and to objects in the game. Also works with PlaymakerFSM events
    /// </summary>
    public class EventListener : MonoBehaviour
    {
        Dictionary<uint, EventListenerWithNameDelegate> listeners = new Dictionary<uint, EventListenerWithNameDelegate>();
        uint _counter = 0;


        public delegate void EventListenerDelegate(GameObject source, GameObject destination);
        public delegate void EventListenerWithNameDelegate(string eventName, GameObject source, GameObject destination);

        bool eventHookAdded = false;

        void OnEventSendInternal(string eventName, GameObject source, GameObject destination, EventManager.EventType eventType)
        {
            OnEventSent(eventName, source, destination, eventType);
            foreach (var listener in listeners)
            {
                try
                {
                    listener.Value(eventName, source, destination);
                }
                catch (Exception e)
                {
                    WeaverLog.LogError("Event Listener Error: " + e);
                }
            }
        }

        /// <summary>
        /// Triggered when any event gets sent
        /// </summary>
        /// <param name="eventName">The event that was sent</param>
        /// <param name="source">The object sending the event</param>
        /// <param name="destination">The object receiving the event</param>
        /// <param name="eventType">The type of event that was sent</param>
        protected virtual void OnEventSent(string eventName, GameObject source, GameObject destination, EventManager.EventType eventType)
        {

        }

        /// <summary>
        /// Adds an event listener to listen for an event
        /// </summary>
        /// <param name="action">The action to be triggered when an event is fired</param>
        /// <returns>>Returns the unique id for the listener. This ID can be used to remove the listener via <see cref="RemoveListener(uint)"/></returns>
        public uint ListenForEvent(EventListenerWithNameDelegate action)
        {
            unchecked
            {
                _counter += 1;
            }
            listeners.Add(_counter, action);
            return _counter;
        }

        /// <summary>
        /// Listens for a specific event fired from any object
        /// </summary>
        /// <param name="eventName">The event to listen for</param>
        /// <param name="action">The action that is called when the event is fired</param>
        /// <returns>Returns the unique id for the listener. This ID can be used to remove the listener via <see cref="RemoveListener(uint)"/></returns>
        public uint ListenForEvent(string eventName, EventListenerDelegate action)
        {
            return ListenForEvent((name, source, dest) =>
            {
                if (name == eventName)
                {
                    action(source, dest);
                }
            });
        }

        /// <summary>
        /// Removes a listener from listening to a certain event
        /// </summary>
        /// <param name="ID">The id of the listener to remove</param>
        public void RemoveListener(uint ID)
        {
            listeners.Remove(ID);
        }

        protected virtual void Awake()
        {
            if (!eventHookAdded)
            {
                eventHookAdded = true;
                EventManager.OnEventTriggered += OnEventSendInternal;
            }
        }

        protected virtual void OnEnable()
        {
            if (!eventHookAdded)
            {
                eventHookAdded = true;
                EventManager.OnEventTriggered += OnEventSendInternal;
            }
        }

        protected virtual void OnDisable()
        {
            if (eventHookAdded)
            {
                eventHookAdded = false;
                EventManager.OnEventTriggered -= OnEventSendInternal;
            }
        }

        protected virtual void OnDestroy()
        {
            if (eventHookAdded)
            {
                eventHookAdded = false;
                EventManager.OnEventTriggered -= OnEventSendInternal;
            }
        }
    }
}
