using UnityEngine;
using WeaverCore.Assets.Components;
using WeaverCore.Attributes;

namespace WeaverCore.Game.Patches
{
	static class HUDCamera_Patches
	{
		[OnInit]
		static void Init()
		{
			On.HUDCamera.OnEnable += HUDCamera_OnEnable;
		}

		private static void HUDCamera_OnEnable(On.HUDCamera.orig_OnEnable orig, HUDCamera self)
		{
			orig(self);
			if (self.GetComponent<SetCameraRect>() == null)
			{
				self.gameObject.AddComponent<RectTransform>();
				self.gameObject.AddComponent<SetCameraRect>();
			}
		}
	}
}
