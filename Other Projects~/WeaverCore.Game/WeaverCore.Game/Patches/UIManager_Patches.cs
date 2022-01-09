using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverCore.Attributes;

namespace WeaverCore.Game.Patches
{

	static class UIManager_Patches
	{
		[OnInit]
		static void Init()
		{
			On.UIManager.DisableScreens += UIManager_DisableScreens;
		}

		private static void UIManager_DisableScreens(On.UIManager.orig_DisableScreens orig, UIManager self)
		{
			orig(self);
			for (int i = 0; i < self.UICanvas.transform.childCount; i++)
			{
				var child = self.UICanvas.transform.GetChild(i);
				if (child.name == "CONTENT GOES HERE" || child.name == "SCENE CONTENT GOES HERE")
				{
					child.gameObject.SetActive(true);
				}
			}
		}
	}
}
