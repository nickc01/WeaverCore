using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Attributes;
using WeaverCore.Interfaces;

namespace WeaverCore.Game.Patches
{


    static class EventRegister_Patches
	{
		private static void EventRegister_SendEvent(On.EventRegister.orig_SendEvent orig, string eventName)
		{
			orig(eventName);
		}

		[OnInit]
		static void Init()
		{
			On.EventRegister.SendEvent += EventRegister_SendEvent;
		}
	}
}
