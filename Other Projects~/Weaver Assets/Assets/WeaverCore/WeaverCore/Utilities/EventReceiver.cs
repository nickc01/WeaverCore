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
	public class EventReceiver : MonoBehaviour
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
		/// An event that is called when an event is received. The string is the event, and the GameObject is the gameObject that sent the event
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
		}

		void OnEnable()
		{
			allReceivers.Add(this);

			/*foreach (var gmHook in GameObjectHooks)
			{
				ReceiveEventsFromObject(gmHook);
			}*/
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

		/*public void ReceiveEventsFromObject(GameObject obj)
		{
			if (obj == null)
			{
				throw new NullReferenceException("GameObject cannot be null");
			}

			Debugger.Log("Receiving from = " + obj.name);
			Debugger.Log("Receiving ID = " + obj.GetInstanceID());

			ReceiveEventsFromObject(obj.GetInstanceID());
		}

		public void ReceiveEventsFromObject(int instanceID)
		{
			impl.ReceiveEventsFromObject(instanceID, gameObject);
			if (!GameObjectHooks.Contains(instanceID))
			{
				GameObjectHooks.Add(instanceID);
			}
		}*/

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
	}
}
