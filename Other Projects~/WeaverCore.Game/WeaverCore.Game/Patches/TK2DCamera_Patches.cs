using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore.Game.Patches
{
	static class TK2DCamera_Patches
	{
		[OnInit(-10)]
		static void Init()
		{
			On.tk2dCamera.Awake += Tk2dCamera_Awake;
			var cam = GameObject.FindObjectOfType<tk2dCamera>();
			if (cam != null)
			{
				cam.gameObject.AddComponent<WeaverCamera>();
			}
		}

		private static void Tk2dCamera_Awake(On.tk2dCamera.orig_Awake orig, tk2dCamera self)
		{
			orig(self);
			self.gameObject.AddComponent<WeaverCamera>();
		}
	}
}
