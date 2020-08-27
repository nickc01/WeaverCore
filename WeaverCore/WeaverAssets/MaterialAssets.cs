using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Assets
{
	public static class MaterialAssets
	{
		public static Material SpriteFlash
		{
			get
			{
				return WeaverAssets.LoadWeaverAsset<Material>("SpriteFlash");
			}
		}
	}
}
