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
		public static AudioClip SwordCling
		{
			get
			{
				return WeaverAssetLoader.LoadWeaverAsset<AudioClip>("SwordCling");
			}
		}
		public static AudioClip DamageEnemy
		{
			get
			{
				return WeaverAssetLoader.LoadWeaverAsset<AudioClip>("EnemyDamage");
			}
		}
		public static AudioClip EnemyDeathBySword
		{
			get
			{
				return WeaverAssetLoader.LoadWeaverAsset<AudioClip>("EnemyDeathSword");
			}
		}

		public static AudioClip Teleport
		{
			get
			{
				return WeaverAssetLoader.LoadWeaverAsset<AudioClip>("Teleport");
			}
		}
		public static AudioClip BossFinalHit
		{
			get
			{
				return WeaverAssetLoader.LoadWeaverAsset<AudioClip>("Boss Final Hit");
			}
		}
		public static AudioClip BossGushing
		{
			get
			{
				return WeaverAssetLoader.LoadWeaverAsset<AudioClip>("Boss Gushing");
			}
		}
		public static AudioClip BossExplosion
		{
			get
			{
				return WeaverAssetLoader.LoadWeaverAsset<AudioClip>("Boss Explosion");
			}
		}
		public static AudioClip BossExplosionUninfected
		{
			get
			{
				return WeaverAssetLoader.LoadWeaverAsset<AudioClip>("Boss Explosion Uninfected");
			}
		}
	}
}
