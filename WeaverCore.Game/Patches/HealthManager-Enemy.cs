using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Features;
using System.IO;
using WeaverCore.Helpers;

namespace WeaverCore.Game.Patches
{
	class HealthManager_Enemy : IPatch
	{
		public void Apply()
		{
			On.HealthManager.Start += HealthManager_Start;
		}

		private void HealthManager_Start(On.HealthManager.orig_Start orig, HealthManager self)
		{
			bool destroyed = false;
			try
			{
				self.gameObject.AddComponent<Enemy>();
				var replacement = Registry.GetAllFeatures<EnemyReplacement>(r => r.EnemyToReplace == self.gameObject.name).FirstOrDefault();

				if (replacement != null)
				{
					var instance = GameObject.Instantiate(replacement.gameObject);
					EventReceiver.ReceiveEventsFromObject(self.gameObject, instance);
					GameObject.Destroy(self.gameObject);
					destroyed = true;
				}
			}
			catch (Exception e)
			{
				Debugger.LogError("Exception occured while spawning enemy replacement : " + e);
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
