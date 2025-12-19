using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Attributes;
using WeaverCore.Interfaces;
using UnityEngine;

namespace WeaverCore.Game.Patches
{


    static class EventRegister_Patches
	{
		private static void EventRegister_SendEvent_string_GameObject(On.EventRegister.orig_SendEvent_string_GameObject orig, string eventName, GameObject excludeGameObject)
		{
			orig(eventName, excludeGameObject);
		}

		[OnInit]
		static void Init()
		{
			On.EventRegister.SendEvent_string_GameObject += EventRegister_SendEvent_string_GameObject;
		}
	}
}
