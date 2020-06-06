﻿using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
	public class GameWeaverEventsImplementation : WeaverEventsImplementation
	{
		public override void BroadcastEvent(string eventName)
		{
			if (PlayMakerFSM.FsmList.Count > 0)
			{
				var e = new FsmEvent(eventName);
				PlayMakerFSM.FsmList[0].Fsm.BroadcastEvent(e);
			}
		}
	}
}
