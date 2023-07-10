using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Assets
{
    /// <summary>
    /// Contains commonly used materials
    /// </summary>
    public static class MaterialAssets
	{
		/// <summary>
		/// Used by <see cref="WeaverCore.Components.SpriteFlasher"/> to flash a sprite
		/// </summary>
		public static Material SpriteFlash
		{
			get
			{
				return WeaverAssets.LoadWeaverAsset<Material>("SpriteFlash");
			}
		}
	}
}
