using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Assets
{
	/// <summary>
	/// Contains a bunch of effect prefabs used througout the game
	/// </summary>
	public static class EffectAssets
	{
		static readonly Dictionary<string, WeakReference> prefabCache = new Dictionary<string, WeakReference>();

		static GameObject LoadCachedPrefab(string assetName)
		{
			return LoadCachedPrefab(assetName, () => WeaverAssets.LoadWeaverAsset<GameObject>(assetName));
		}

		static GameObject LoadCachedPrefab(string assetName, Func<GameObject> loadFunction)
		{
			if (prefabCache.TryGetValue(assetName, out var weakRef) && weakRef.Target is GameObject cachedPrefab && cachedPrefab != null)
			{
				return cachedPrefab;
			}

			var loadedPrefab = loadFunction();
			prefabCache[assetName] = new WeakReference(loadedPrefab);
			return loadedPrefab;
		}

		public static GameObject NailStrikePrefab { get { return LoadCachedPrefab("Nail Strike"); } }
		public static GameObject SharpShadowImpactPrefab { get { return LoadCachedPrefab("Sharp Shadow Impact"); } }
		public static GameObject FireballHitPrefab { get { return LoadCachedPrefab("Fireball Hit"); } }
		//public static GameObject SlashGhost1Prefab { get { return WeaverAssets.LoadWeaverAsset<GameObject>("Slash Ghost 1"); } }
		//public static GameObject SlashGhost2Prefab { get { return WeaverAssets.LoadWeaverAsset<GameObject>("Slash Ghost 2"); } }
		public static GameObject SlashImpactPrefab { get { return LoadCachedPrefab("Slash Impact"); } }
		public static GameObject UninfectedDeathPrefab { get { return LoadCachedPrefab("Uninfected Death Pt"); } }
		public static GameObject UninfectedHitPrefab { get { return LoadCachedPrefab("Uninfected Hit Pt"); } }
		public static GameObject BlockedHitPrefab { get { return LoadCachedPrefab("Blocked Hit"); } }

		public static GameObject SlashGhost1Prefab { get { return LoadCachedPrefab("Slash Ghost 1"); } }
		public static GameObject SlashGhost2Prefab { get { return LoadCachedPrefab("Slash Ghost 2"); } }

		public static GameObject TeleportGlowPrefab { get { return LoadCachedPrefab("Death Glow"); } }
		public static GameObject TeleLinePrefab { get { return LoadCachedPrefab("Tele Line"); } }
		public static GameObject WhiteFlashPrefab { get { return LoadCachedPrefab("White Flash Default", () => WeaverAssets.LoadWeaverAssets<GameObject>("White Flash Default").First(g => g.name == "White Flash Default")); } }
	}
}
