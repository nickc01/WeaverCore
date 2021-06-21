using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeaverCore.Utilities
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
			OnEventSent(eventName,source,destination, eventType);
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

		protected virtual void OnEventSent(string eventName, GameObject source, GameObject destination, EventManager.EventType eventType)
		{
			//WeaverLog.Log($"Event Received = [{eventName}] [{source?.name}] [{destination?.name}] [{eventType}]");
		}

		public uint ListenForEvent(EventListenerWithNameDelegate action)
		{
			unchecked
			{
				_counter += 1;
			}
			listeners.Add(_counter, action);
			return _counter;
		}

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

	/*public class EventReceiverOLD : MonoBehaviour
	{
		//List<int> GameObjectHooks = new List<int>();
		List<string> EventHooks = new List<string>();

		static HashSet<EventReceiver> allReceivers = new HashSet<EventReceiver>();

		public static IEnumerable<EventReceiver> AllReceivers
		{
			get
			{
				return allReceivers;
			}
		}

		/// <summary>
		/// Called when an event is received. The string is the event, and the GameObject is the gameObject that sent the event
		/// </summary>
		public event Action<string,GameObject> OnReceiveEvent;

		static EventReceiver_I impl;

		static EventReceiver()
		{
			impl = ImplFinder.GetImplementation<EventReceiver_I>();
			impl.Initialize();
		}

		void Start()
		{
			allReceivers.Add(this);
			OnReceiveEvent += EventReceiver_OnReceiveEvent;
			ReceiveAllEventsFromName("TOOK DAMAGE");
		}

		private void EventReceiver_OnReceiveEvent(string arg1, GameObject arg2)
		{
			WeaverLog.Log($"Receive Event [{arg1}] from source [{arg2.name}]");
		}

		void OnEnable()
		{
			allReceivers.Add(this);
			foreach (var eventHook in EventHooks)
			{
				ReceiveAllEventsFromName(eventHook);
			}
		}

		void OnDisable()
		{
			allReceivers.Remove(this);
			StopReceiver();
		}

		public void ReceiveAllEventsFromName(string eventName)
		{
			impl.ReceiveAllEventsOfName(eventName, gameObject);
			if (!EventHooks.Contains(eventName))
			{
				EventHooks.Add(eventName);
			}
		}

		public void ReceiveEvent(string eventName,GameObject source)
		{
			if (enabled)
			{
				if (OnReceiveEvent != null)
				{
					OnReceiveEvent.Invoke(eventName, source);
				}
			}
		}

		void StopReceiver()
		{
			impl.RemoveReceiver(gameObject);
		}

		void OnDestroy()
		{
			allReceivers.Remove(this);
			StopReceiver();
		}
	}*/
}
