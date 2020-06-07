using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Game.Patches
{
	public class EventRegistryPatch : IPatch
	{
		public void Apply()
		{
			On.EventRegister.SendEvent += EventRegister_SendEvent;
		}

		private void EventRegister_SendEvent(On.EventRegister.orig_SendEvent orig, string eventName)
		{
			//Debugger.Log("Event Triggered = " + eventName);
			orig(eventName);
		}
	}
}
