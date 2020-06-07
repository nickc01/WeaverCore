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
		public event Action<string> OnReceiveEvent;

		static EventReceiver_I hijackerInternal;

		static EventReceiver()
		{
			hijackerInternal = ImplFinder.GetImplementation<EventReceiver_I>();
			hijackerInternal.Initialize(ModuleInitializer.weaverCorePatcher);
		}


		public static void ReceiveEventsFromObject(GameObject obj, GameObject toSendTo)
		{
			hijackerInternal.ReceiveEventsFromObject(obj, toSendTo);
		}

		public static void SendEvent(GameObject destination, string eventName)
		{
			hijackerInternal.SendEvent(destination, eventName);
		}

		public static void BroadcastEvent(string eventName)
		{
			hijackerInternal.BroadcastEvent(eventName);
		}

		public void ReceiveAllEventsFromName(string eventName)
		{
			hijackerInternal.ReceiveAllEventsOfName(eventName, gameObject);
		}

		public void ReceiveEvent(string eventName)
		{
			OnReceiveEvent?.Invoke(eventName);
		}

		public static void RemoveAllReceivers(GameObject destination)
		{
			hijackerInternal.RemoveReceiver(destination);
		}

		void OnDestroy()
		{
			RemoveAllReceivers(gameObject);
		}
	}
}
