using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.WeaverAssets
{
	public static class AudioAssets
	{
		public static AudioClip SwordCling => WeaverAssetLoader.LoadWeaverAsset<AudioClip>("SwordCling");
		public static AudioClip DamageEnemy => WeaverAssetLoader.LoadWeaverAsset<AudioClip>("EnemyDamage");
		public static AudioClip EnemyDeathBySword => WeaverAssetLoader.LoadWeaverAsset<AudioClip>("EnemyDeathSword");

		public static AudioClip Teleport => WeaverAssetLoader.LoadWeaverAsset<AudioClip>("Teleport");
		public static AudioClip BossFinalHit => WeaverAssetLoader.LoadWeaverAsset<AudioClip>("Boss Final Hit");
		public static AudioClip BossGushing => WeaverAssetLoader.LoadWeaverAsset<AudioClip>("Boss Gushing");
		public static AudioClip BossExplosion => WeaverAssetLoader.LoadWeaverAsset<AudioClip>("Boss Explosion");
		public static AudioClip BossExplosionUninfected => WeaverAssetLoader.LoadWeaverAsset<AudioClip>("Boss Explosion Uninfected");
	}
}
