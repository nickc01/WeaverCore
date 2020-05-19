using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Helpers;

namespace WeaverCore.WeaverAssets
{
	public static class MaterialAssets
	{
		public static Material SpriteFlash => WeaverAssetLoader.LoadWeaverAsset<Material>("SpriteFlash");
	}
}
