using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Attributes;

namespace WeaverCore.Game.Patches
{
	static class GameManager_SetState
	{
		[OnInit]
		static void Init()
		{
			On.GameManager.SetState += SetStatePatch;
		}

		private static void SetStatePatch(On.GameManager.orig_SetState orig, GameManager self, GlobalEnums.GameState newState)
		{
			orig(self,newState);
			WeaverGameManager.TriggerGameStateChange();
		}
	}
}
