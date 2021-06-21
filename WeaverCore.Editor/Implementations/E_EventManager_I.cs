using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Implementations;

using System.Collections.Generic;

namespace WeaverCore.Editor.Implementations
{
	public class E_EventManager_I : EventManager_I
	{
		public class E_Statics : Statics
		{
			public override void BroadcastToPlaymakerFSMs(string eventName, GameObject source, bool skipEventManagers)
			{
				//Nothing extra needs to be done in the editor
			}

			public override void TriggerEventToGameObjectPlaymakerFSMs(string eventName, GameObject destination, GameObject source, bool skipEventManagers)
			{
				//Nothing extra needs to be done in the editor
			}
		}
	}
	/*public class E_EventReceiver_I : EventReceiver_I
	{
		Dictionary<string, List<GameObject>> EventHooks = new Dictionary<string, List<GameObject>>();
		//Dictionary<int, List<GameObject>> GameObjectHooks = new Dictionary<int, List<GameObject>>();

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

		public override void Initialize()
		{
			//Nothing Extra Needed
		}

		public override void RemoveReceiver(GameObject obj)
		{

			foreach (var entry in EventHooks)
			{
				if (entry.Value != null)
				{
					entry.Value.Remove(obj);
					//TODO - OPTIMIZE BY REMOVING KEY WHEN LIST IS EMPTY
				}
			}
		}
	}*/
}
