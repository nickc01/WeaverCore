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
	public class G_EventManager_I : EventManager_I
	{
		static List<GameObject> ExcludedEventManagerObjects = new List<GameObject>();

		static bool skipEventManagerBroadcast = false;

		public static Statics StaticImpl = new G_Statics();

		static GameObject customSource = null;

		[OnInit]
		static void Init()
		{
			On.HutongGames.PlayMaker.Fsm.Event_FsmEventTarget_FsmEvent += Fsm_Event_FsmEventTarget_FsmEvent;
		}

		public class G_Statics : Statics
		{
			public override void BroadcastToPlaymakerFSMs(string eventName, GameObject source, bool skipEventManagers)
			{
				if (PlayMakerFSM.FsmList.Count > 0)
				{
					for (int i = PlayMakerFSM.FsmList.Count - 1; i >= 0; i--)
					{
						if (i >= PlayMakerFSM.FsmList.Count)
						{
							continue;
						}
						var fsmComponent = PlayMakerFSM.FsmList[i];
						if (fsmComponent.Fsm != null)
						{
							customSource = source;
							if (skipEventManagers)
							{
								skipEventManagerBroadcast = true;
								ExcludedEventManagerObjects.Add(fsmComponent.gameObject);
							}
							try
							{
								fsmComponent.Fsm.Event(eventName);
							}
							finally
							{
								if (customSource == source)
								{
									customSource = null;
								}
								if (skipEventManagers)
								{
									skipEventManagerBroadcast = false;
									ExcludedEventManagerObjects.Remove(fsmComponent.gameObject);
								}
							}
						}
					}
				}
			}

			public override void TriggerEventToGameObjectPlaymakerFSMs(string eventName, GameObject destination, GameObject source, bool skipEventManagers)
			{
				var FSMs = destination.GetComponents<PlayMakerFSM>();

				if (FSMs != null && FSMs.GetLength(0) > 0)
				{
					foreach (var fsmComponent in FSMs)
					{
						customSource = source;
						if (skipEventManagers)
						{
							ExcludedEventManagerObjects.Add(destination);
						}
						try
						{
							fsmComponent.Fsm.Event(eventName);
						}
						finally
						{
							if (customSource == source)
							{
								customSource = null;
							}
							if (skipEventManagers)
							{
								ExcludedEventManagerObjects.Remove(destination);
							}
						}
					}
				}
			}
		}

		static readonly FsmEventTarget SelfTarget = new FsmEventTarget();

		private static void Fsm_Event_FsmEventTarget_FsmEvent(On.HutongGames.PlayMaker.Fsm.orig_Event_FsmEventTarget_FsmEvent orig, Fsm self, FsmEventTarget eventTarget, FsmEvent fsmEvent)
		{
			try
			{
				if (eventTarget == null)
				{
					eventTarget = SelfTarget;
				}
				var source = self.GameObject;
				if (customSource != null)
				{
					source = customSource;
				}
				if (fsmEvent != null)
				{
					switch (eventTarget.target)
					{
						case FsmEventTarget.EventTarget.Self:
							TriggerEvent(self.GameObject, fsmEvent.Name, source);
							break;
						case FsmEventTarget.EventTarget.GameObject:
							TriggerEvent(self.GetOwnerDefaultTarget(eventTarget.gameObject), fsmEvent.Name, source);
							break;
						case FsmEventTarget.EventTarget.GameObjectFSM:
							TriggerEvent(self.GetOwnerDefaultTarget(eventTarget.gameObject), fsmEvent.Name, source);
							break;
						case FsmEventTarget.EventTarget.FSMComponent:
							if (eventTarget.fsmComponent != null)
							{
								TriggerEvent(eventTarget.fsmComponent.gameObject, fsmEvent.Name, source);
							}
							break;
						case FsmEventTarget.EventTarget.BroadcastAll:
							BroadcastEvent(fsmEvent.Name, source);
							break;
						case FsmEventTarget.EventTarget.HostFSM:
							if (self.Host != null)
							{
								TriggerEvent(self.Host.GameObject, fsmEvent.Name, source);
							}
							break;
						case FsmEventTarget.EventTarget.SubFSMs:
							foreach (var fsm in new List<Fsm>(self.SubFsmList))
							{
								TriggerEvent(fsm.GameObject, fsmEvent.Name, source);
							}
							break;
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

		static void TriggerEvent(GameObject targetObject, string eventName, GameObject source)
		{
			try
			{
				if (targetObject != null && !ExcludedEventManagerObjects.Contains(targetObject))
				{
					var receiver = targetObject.GetComponent<G_EventManager_I>();
					if (receiver != null)
					{
						receiver.TriggerEventInternal(eventName, source);
					}
				}
			}
			finally
			{
				RegisterTriggeredEvent(eventName, source, targetObject, EventManager.EventType.Message);
			}
			
		}

		static void BroadcastEvent(string eventName, GameObject source)
		{
			if (!skipEventManagerBroadcast)
			{
				try
				{
					BroadcastEventInternal(eventName, source);
				}
				finally
				{
					RegisterTriggeredEvent(eventName, source, null, EventManager.EventType.Broadcast);
				}
			}
		}
	}
}