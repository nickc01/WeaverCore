using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Helpers;

namespace WeaverCore.WeaverAssets
{
	public static class AudioAssets
	{
		public static AudioClip SwordCling => WeaverAssetLoader.LoadWeaverAsset<AudioClip>("SwordCling");
		public static AudioClip DamageEnemy => WeaverAssetLoader.LoadWeaverAsset<AudioClip>("EnemyDamage");
		public static AudioClip EnemyDeathBySword => WeaverAssetLoader.LoadWeaverAsset<AudioClip>("EnemyDeathSword");
	}
}
