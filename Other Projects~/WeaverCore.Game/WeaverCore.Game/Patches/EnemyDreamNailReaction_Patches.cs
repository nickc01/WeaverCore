using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.Interfaces;

namespace WeaverCore.Game.Patches
{
	class EnemyDreamNailReaction_Patches
	{
		[OnInit]
		static void Init()
		{
			On.EnemyDreamnailReaction.ShowConvo += EnemyDreamnailReaction_ShowConvo;
		}

		private static void EnemyDreamnailReaction_ShowConvo(On.EnemyDreamnailReaction.orig_ShowConvo orig, EnemyDreamnailReaction self)
		{
			var flasher = self.GetComponent<SpriteFlasher>();
			if (flasher != null)
			{
				flasher.DoFlash(0.01f, 0.75f, 0.9f, Color.white, 0.25f);
			}
			orig(self);
		}
	}
}
