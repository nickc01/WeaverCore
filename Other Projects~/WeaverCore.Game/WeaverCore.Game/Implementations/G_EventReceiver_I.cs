using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;
using WeaverCore.Attributes;

namespace WeaverCore.Game.Implementations
{
	public class G_EventReceiver_I : EventReceiver_I
	{
		protected class EventInfo
		{
			public string eventName;
			public GameObject destination;
		}

		[OnInit]
		static void Init()
		{
			On.HutongGames.PlayMaker.Fsm.Event_FsmEventTarget_FsmEvent += Fsm_Event_FsmEventTarget_FsmEvent;
		}

		protected static List<EventInfo> EventHooks = new List<EventInfo>();

		//static MethodInfo EventPrefixMethod;

		public override void Initialize()
		{

		}

		private static void Fsm_Event_FsmEventTarget_FsmEvent(On.HutongGames.PlayMaker.Fsm.orig_Event_FsmEventTarget_FsmEvent orig, Fsm self, FsmEventTarget eventTarget, FsmEvent fsmEvent)
		{
			try
			{
				if (fsmEvent != null)
				{
					foreach (var eventHook in EventHooks)
					{
						if (fsmEvent.Name == eventHook.eventName)
						{
							var receiver = eventHook.destination.GetComponent<EventReceiver>();
							if (receiver != null)
							{
								receiver.ReceiveEvent(fsmEvent.Name, self.GameObject);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				WeaverLog.Log("EVENT ERROR = " + e);
			}
			finally
			{
				orig(self, eventTarget, fsmEvent);
			}
		}

		static bool EventPrefix(Fsm __instance, FsmEventTarget eventTarget, FsmEvent fsmEvent)
		{
			try
			{
				if (fsmEvent == null)
				{
					return true;
				}
				foreach (var eventHook in EventHooks)
				{
					if (fsmEvent.Name == eventHook.eventName)
					{
						var receiver = eventHook.destination.GetComponent<EventReceiver>();
						if (receiver != null)
						{
							receiver.ReceiveEvent(fsmEvent.Name,__instance.GameObject);
						}
					}
				}
				return true;
			}
			catch (Exception e)
			{
				WeaverLog.Log("EVENT ERROR = " + e);
			}
			return true;
		}

		public override void ReceiveAllEventsOfName(string name, GameObject destination)
		{
			if (destination == null)
			{
				throw new Exception("The gameObject cannot be null");
			}
			EventHooks.Add(new EventInfo() { eventName = name, destination = destination });
		}

		public override void RemoveReceiver(GameObject destination)
		{
			EventHooks.RemoveAll(hook => hook.destination == destination);
		}
	}
}