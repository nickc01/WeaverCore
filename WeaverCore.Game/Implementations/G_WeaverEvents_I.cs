using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Implementations
{
	public class G_WeaverEvents_I : WeaverEvents_I
	{
		public override void BroadcastEvent(string eventName,GameObject source)
		{
			//Debugger.Log("Broadcasting Event = " + eventName);
			foreach (var receiver in EventReceiver.AllReceivers)
			{
				receiver.ReceiveEvent(eventName,source);
			}

			if (PlayMakerFSM.FsmList.Count > 0)
			{
				var fsmEvent = new FsmEvent(eventName);

				foreach (var fsmComponent in PlayMakerFSM.FsmList)
				{
					if (fsmComponent.Fsm != null)
					{
						//Debugger.Log("FSMComponent = " + fsmComponent);
						fsmComponent.Fsm.ProcessEvent(FsmEvent.GetFsmEvent(eventName));
					}
				}
			}
		}

		public override void SendEventToObject(GameObject gameObject, string eventName)
		{
			EventReceiver receiver = null;
			if ((receiver = gameObject.GetComponent<EventReceiver>()) != null)
			{
				receiver.ReceiveEvent(eventName,gameObject);
			}

			var FSMs = gameObject.GetComponents<PlayMakerFSM>();

			if (FSMs != null && FSMs.GetLength(0) > 0)
			{
				var fsmEvent = new FsmEvent(eventName);

				foreach (var fsmComponent in FSMs)
				{
					fsmComponent.Fsm.ProcessEvent(fsmEvent);
				}
			}
		}
	}
}
