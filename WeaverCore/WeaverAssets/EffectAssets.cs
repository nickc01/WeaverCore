using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Helpers;

namespace WeaverCore.WeaverAssets
{
	public static class EffectAssets
	{
		public static GameObject NailStrikePrefab => WeaverAssetLoader.LoadWeaverAsset<GameObject>("Nail Strike");
		public static GameObject SlashGhost1Prefab => WeaverAssetLoader.LoadWeaverAsset<GameObject>("Slash Ghost 1");
		public static GameObject SlashGhost2Prefab => WeaverAssetLoader.LoadWeaverAsset<GameObject>("Slash Ghost 2");
		public static GameObject SlashImpactPrefab => WeaverAssetLoader.LoadWeaverAsset<GameObject>("Slash Impact");
		public static GameObject SpellHitPrefab => WeaverAssetLoader.LoadWeaverAsset<GameObject>("Spell HIt");
		public static GameObject UninfectedDeathPrefab => WeaverAssetLoader.LoadWeaverAsset<GameObject>("Uninfected Death Pt");
		public static GameObject UninfectedHitPrefab => WeaverAssetLoader.LoadWeaverAsset<GameObject>("Uninfected Hit Pt");
		public static GameObject BlockedHitPrefab => WeaverAssetLoader.LoadWeaverAsset<GameObject>("Blocked Hit");

		public static GameObject GhostSlash1 => WeaverAssetLoader.LoadWeaverAsset<GameObject>("Slash Ghost 1");
		public static GameObject GhostSlash2 => WeaverAssetLoader.LoadWeaverAsset<GameObject>("Slash Ghost 2");
	}
}
