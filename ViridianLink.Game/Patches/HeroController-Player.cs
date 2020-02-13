using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViridianLink.Game.Patches
{
	public class HeroController_Player : IPatch
	{
		public void Apply()
		{
			On.HeroController.Start += HeroController_Start;
		}

		private void HeroController_Start(On.HeroController.orig_Start orig, HeroController self)
		{
			self.gameObject.AddComponent<Player>();
			orig(self);
		}
	}
}
