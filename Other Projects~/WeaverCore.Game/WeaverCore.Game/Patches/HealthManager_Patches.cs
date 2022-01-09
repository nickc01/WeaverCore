using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Features;
using System.IO;
using WeaverCore.Utilities;
using System.Reflection;
using System.Security.Permissions;
using WeaverCore.Interfaces;
using WeaverCore.Attributes;

namespace WeaverCore.Game.Patches
{
	class HealthManager_Enemy
	{
		[OnInit]
		static void Init()
		{
			On.HealthManager.Start += HealthManager_Start;
		}

		private static void HealthManager_Start(On.HealthManager.orig_Start orig, HealthManager self)
		{
			bool destroyed = false;
			try
			{
				destroyed = EntityReplacements.ReplaceObject(self.gameObject, out var _);
			}
			catch (Exception e)
			{
				WeaverLog.LogError("Exception occured while spawning entity replacement : " + e);
			}
			finally
			{
				if (!destroyed)
				{
					orig(self);
				}
			}
		}
	}
}
