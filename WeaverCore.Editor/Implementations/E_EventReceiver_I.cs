using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Implementations;
using static WeaverCore.Utilities.Harmony;
using System.Collections.Generic;

namespace WeaverCore.Editor.Implementations
{
	public class E_EventReceiver_I : EventReceiver_I
	{
		Dictionary<string, List<GameObject>> EventHooks = new Dictionary<string, List<GameObject>>();
		Dictionary<int, List<GameObject>> GameObjectHooks = new Dictionary<int, List<GameObject>>();

		public override void ReceiveAllEventsOfName(string eventName, GameObject destination)
		{
			List<GameObject> HookList;
			if (!EventHooks.ContainsKey(eventName))
			{
				EventHooks.Add(eventName, HookList = new List<GameObject>());
			}
			else
			{
				HookList = EventHooks[eventName];
			}
			if (!HookList.Contains(destination))
			{
				HookList.Add(destination);
			}
		}

		public override void Initialize(HarmonyInstance patcher)
		{
			//Nothing Extra Needed
		}

		public override void RemoveReceiver(GameObject obj)
		{
			foreach (var entry in GameObjectHooks)
			{
				if (entry.Value != null)
				{
					entry.Value.Remove(obj);
					//TODO - OPTIMIZE BY REMOVING KEY WHEN LIST IS EMPTY
				}
			}

			foreach (var entry in EventHooks)
			{
				if (entry.Value != null)
				{
					entry.Value.Remove(obj);
					//TODO - OPTIMIZE BY REMOVING KEY WHEN LIST IS EMPTY
				}
			}
		}

		public override void ReceiveEventsFromObject(int instanceID, GameObject destination)
		{
			List<GameObject> HookList;
			if (!GameObjectHooks.ContainsKey(instanceID))
			{
				GameObjectHooks.Add(instanceID,HookList = new List<GameObject>());
			}
			else
			{
				HookList = GameObjectHooks[instanceID];
			}
			if (!HookList.Contains(destination))
			{
				HookList.Add(destination);
			}
		}

		public override void OnReceiveEvent(EventReceiver receiver, string eventName)
		{
			if (GameObjectHooks.ContainsKey(receiver.gameObject.GetInstanceID()))
			{
				var list = GameObjectHooks[receiver.gameObject.GetInstanceID()];
				if (list != null)
				{
					foreach (var destination in list)
					{
						var destReceiver = destination.GetComponent<EventReceiver>();
						if (destReceiver != null)
						{
							destReceiver.ReceiveEvent(eventName);
						}
					}
				}
			}

			if (EventHooks.ContainsKey(eventName))
			{
				var list = EventHooks[eventName];
				if (list != null)
				{
					foreach (var destination in list)
					{
						var destReceiver = destination.GetComponent<EventReceiver>();
						if (destReceiver != null)
						{
							destReceiver.ReceiveEvent(eventName);
						}
					}
				}
			}

		}
	}
}
