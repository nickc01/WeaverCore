using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Assets
{
	public static class EffectAssets
	{
		public static GameObject NailStrikePrefab { get { return WeaverAssets.LoadWeaverAsset<GameObject>("Nail Strike"); } }
		public static GameObject SharpShadowImpactPrefab { get { return WeaverAssets.LoadWeaverAsset<GameObject>("Sharp Shadow Impact"); } }
		public static GameObject FireballHitPrefab { get { return WeaverAssets.LoadWeaverAsset<GameObject>("Fireball Hit"); } }
		public static GameObject SlashGhost1Prefab { get { return WeaverAssets.LoadWeaverAsset<GameObject>("Slash Ghost 1"); } }
		public static GameObject SlashGhost2Prefab { get { return WeaverAssets.LoadWeaverAsset<GameObject>("Slash Ghost 2"); } }
		public static GameObject SlashImpactPrefab { get { return WeaverAssets.LoadWeaverAsset<GameObject>("Slash Impact"); } }
		public static GameObject SpellHitPrefab { get { return WeaverAssets.LoadWeaverAsset<GameObject>("Spell HIt"); } }
		public static GameObject UninfectedDeathPrefab { get { return WeaverAssets.LoadWeaverAsset<GameObject>("Uninfected Death Pt"); } }
		public static GameObject UninfectedHitPrefab { get { return WeaverAssets.LoadWeaverAsset<GameObject>("Uninfected Hit Pt"); } }
		public static GameObject BlockedHitPrefab { get { return WeaverAssets.LoadWeaverAsset<GameObject>("Blocked Hit"); } }

		public static GameObject GhostSlash1Prefab { get { return WeaverAssets.LoadWeaverAsset<GameObject>("Slash Ghost 1"); } }
		public static GameObject GhostSlash2Prefab { get { return WeaverAssets.LoadWeaverAsset<GameObject>("Slash Ghost 2"); } }

		public static GameObject TeleportGlowPrefab { get { return WeaverAssets.LoadWeaverAsset<GameObject>("Death Glow"); } }
		public static GameObject TeleLinePrefab { get { return WeaverAssets.LoadWeaverAsset<GameObject>("Tele Line"); } }
		public static GameObject WhiteFlashPrefab { get { return WeaverAssets.LoadWeaverAsset<GameObject>("White Flash"); } }
	}
}
