using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.WeaverAssets
{
	public static class MaterialAssets
	{
		public static Material SpriteFlash
		{
			get
			{
				return WeaverAssetLoader.LoadWeaverAsset<Material>("SpriteFlash");
			}
		}
	}
}
