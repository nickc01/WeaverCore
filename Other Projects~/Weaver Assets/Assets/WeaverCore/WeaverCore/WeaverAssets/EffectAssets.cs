using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.WeaverAssets
{
	public static class EffectAssets
	{
		public static GameObject NailStrikePrefab { get { return WeaverAssetLoader.LoadWeaverAsset<GameObject>("Nail Strike"); } }
		public static GameObject SlashGhost1Prefab { get { return WeaverAssetLoader.LoadWeaverAsset<GameObject>("Slash Ghost 1"); } }
		public static GameObject SlashGhost2Prefab { get { return WeaverAssetLoader.LoadWeaverAsset<GameObject>("Slash Ghost 2"); } }
		public static GameObject SlashImpactPrefab { get { return WeaverAssetLoader.LoadWeaverAsset<GameObject>("Slash Impact"); } }
		public static GameObject SpellHitPrefab { get { return WeaverAssetLoader.LoadWeaverAsset<GameObject>("Spell HIt"); } }
		public static GameObject UninfectedDeathPrefab { get { return WeaverAssetLoader.LoadWeaverAsset<GameObject>("Uninfected Death Pt"); } }
		public static GameObject UninfectedHitPrefab { get { return WeaverAssetLoader.LoadWeaverAsset<GameObject>("Uninfected Hit Pt"); } }
		public static GameObject BlockedHitPrefab { get { return WeaverAssetLoader.LoadWeaverAsset<GameObject>("Blocked Hit"); } }

		public static GameObject GhostSlash1Prefab { get { return WeaverAssetLoader.LoadWeaverAsset<GameObject>("Slash Ghost 1"); } }
		public static GameObject GhostSlash2Prefab { get { return WeaverAssetLoader.LoadWeaverAsset<GameObject>("Slash Ghost 2"); } }

		public static GameObject TeleportGlowPrefab { get { return WeaverAssetLoader.LoadWeaverAsset<GameObject>("Death Glow"); } }
		public static GameObject TeleLinePrefab { get { return WeaverAssetLoader.LoadWeaverAsset<GameObject>("Tele Line"); } }
		public static GameObject WhiteFlashPrefab { get { return WeaverAssetLoader.LoadWeaverAsset<GameObject>("White Flash"); } }
	}
}
