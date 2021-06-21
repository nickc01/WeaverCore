using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;
using WeaverCore.Internal;

namespace WeaverCore.Utilities
{
	public sealed class EventManager : MonoBehaviour
	{
		EventManager_I impl;
		static EventManager_I.Statics implStatics = ImplFinder.GetImplementation<EventManager_I.Statics>();

		static HashSet<EventManager> allReceivers = new HashSet<EventManager>();

		/// <summary>
		/// A delegate used for receiving events
		/// </summary>
		/// <param name="eventName">The name of the event received</param>
		/// <param name="source"></param>
		public delegate void EventReceiveDelegate(string eventName, GameObject source);

		/// <summary>
		/// Called whenever an event is received
		/// </summary>
		public event EventReceiveDelegate OnReceivedEvent;

		List<(string eventName, Action<GameObject> source)> eventSpecificHooks = new List<(string eventName, Action<GameObject> source)>();

		/// <summary>
		/// Executes an action whenever an event with the specified <paramref name="name"/> is received
		/// </summary>
		/// <param name="name">The event name</param>
		/// <param name="action">The action to execute when the event of the specified name is received</param>
		public void AddReceiverForEvent(string name, Action action)
		{
			eventSpecificHooks.Add((name,g => action()));
		}

		/// <summary>
		/// Executes an action whenever an event with the specified <paramref name="name"/> is received
		/// </summary>
		/// <param name="name">The event name</param>
		/// <param name="action">The action to execute when the event of the specified name is received. The gameObject parameter is the source gameObject</param>
		public void AddReceiverForEvent(string name, Action<GameObject> action)
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

		public void TriggerEvent(string eventName, GameObject source)
		{
			TriggerEventInternal(eventName, source);
			implStatics.TriggerEventToGameObjectPlaymakerFSMs(eventName, gameObject, source,true);
		}

		public static void BroadcastEvent(string eventName,GameObject source)
		{
			BroadcastEventInternal(eventName, source);
			implStatics.BroadcastToPlaymakerFSMs(eventName, source, true);
		}

		public static void SendEventToGameObject(string eventName, GameObject destination, GameObject source = null)
		{
			foreach (var receiver in destination.GetComponents<EventManager>())
			{
				receiver.TriggerEventInternal(eventName, source);
			}
			implStatics.TriggerEventToGameObjectPlaymakerFSMs(eventName, destination, source, true);
		}

		static bool HasAtLeastOneFSMComponent(GameObject obj)
		{
			if (PlayMakerUtilities.PlayMakerAvailable)
			{
				var FSMComponent = obj.GetComponent(PlayMakerUtilities.PlayMakerFSMType);
				return FSMComponent != null;
			}
			return false;
		}

		internal void TriggerEventInternal(string eventName, GameObject source)
		{
			OnReceivedEvent?.Invoke(eventName, source);
			foreach (var pair in eventSpecificHooks)
			{
				if (pair.eventName == eventName)
				{
					pair.source?.Invoke(source);
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
						pair.source?.Invoke(source);
					}
				}
			}
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
