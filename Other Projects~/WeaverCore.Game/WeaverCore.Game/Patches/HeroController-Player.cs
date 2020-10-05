using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Attributes;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Patches
{
	static class HeroController_Player
	{
		[OnInit]
		static void Init()
		{
			On.HeroController.Start += HeroController_Start;
		}

		static void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
		{
			self.gameObject.AddComponent<Player>();
			orig(self);
		}
	}
}
