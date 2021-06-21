using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Interfaces;


namespace WeaverCore.Implementations
{
	public abstract class EventManager_I : MonoBehaviour, IImplementation
    {
		public abstract class Statics : IImplementation
		{
			public static bool SkipEventManagerReceives = false;

			public abstract void BroadcastToPlaymakerFSMs(string eventName, GameObject source, bool skipEventManagers);

			public void BroadcastEventInternal(string eventName, GameObject source)
			{
				EventManager.BroadcastEventInternal(eventName, source);
			}

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

		/// <summary>
		/// Sends the event to any playmaker FSMs that are on the current object
		/// </summary>
		/// <param name="eventName">The name of the event</param>
		/// <param name="source">The gameObject the event came from</param>
		//public abstract void TriggerEventToPlaymakerFSMs(string eventName, GameObject source);
	}
}
