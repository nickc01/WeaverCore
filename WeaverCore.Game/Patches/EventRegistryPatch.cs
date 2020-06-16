using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Interfaces;

namespace WeaverCore.Game.Patches
{
	public class EventRegistryPatch : IInit
	{
		private void EventRegister_SendEvent(On.EventRegister.orig_SendEvent orig, string eventName)
		{
			//Debugger.Log("Event Triggered = " + eventName);
			orig(eventName);
		}

		void IInit.OnInit()
		{
			On.EventRegister.SendEvent += EventRegister_SendEvent;
		}
	}
}
