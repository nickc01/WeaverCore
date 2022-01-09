using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Interfaces;


namespace WeaverCore.Implementations
{
    public abstract class EventManager_I : MonoBehaviour, IImplementation
    {
		public abstract class Statics : IImplementation
		{
			public static bool SkipEventManagerReceives = false;

			public abstract void BroadcastToPlaymakerFSMs(string eventName, GameObject source, bool skipEventManagers);

			public abstract void TriggerEventToGameObjectPlaymakerFSMs(string eventName, GameObject destination, GameObject source, bool skipEventManagers);
		}

        protected EventManager receiver;

		protected virtual void Awake()
		{
			receiver = GetComponent<EventManager>();
		}

        protected internal void TriggerEventInternal(string eventName, GameObject source)
		{
			receiver.TriggerEventInternal(eventName, source);
		}

		internal protected static void RegisterTriggeredEvent(string eventName, GameObject source, GameObject destination, EventManager.EventType eventType)
		{
			EventManager.RegisterTriggeredEvent(eventName, source, destination, eventType);
		}

		internal protected static void BroadcastEventInternal(string eventName, GameObject source)
		{
			EventManager.BroadcastEventInternal(eventName, source);
		}
	}
}
