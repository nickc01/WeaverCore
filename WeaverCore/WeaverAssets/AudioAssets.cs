using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Assets
{
	/// <summary>
	/// Contains a bunch of misc audio sounds used througout the game
	/// </summary>
	public static class AudioAssets
	{
		public static AudioClip SwordCling
		{
			get
			{
				return WeaverAssets.LoadWeaverAsset<AudioClip>("SwordCling");
			}
		}
		public static AudioClip DamageEnemy
		{
			get
			{
				return WeaverAssets.LoadWeaverAsset<AudioClip>("EnemyDamage");
			}
		}
		public static AudioClip EnemyDeathBySword
		{
			get
			{
				return WeaverAssets.LoadWeaverAsset<AudioClip>("EnemyDeathSword");
			}
		}

		public static AudioClip Teleport
		{
			get
			{
				return WeaverAssets.LoadWeaverAsset<AudioClip>("Teleport");
			}
		}
		public static AudioClip BossFinalHit
		{
			get
			{
				return WeaverAssets.LoadWeaverAsset<AudioClip>("Boss Final Hit");
			}
		}
		public static AudioClip BossGushing
		{
			get
			{
				return WeaverAssets.LoadWeaverAsset<AudioClip>("Boss Gushing");
			}
		}
		public static AudioClip BossExplosion
		{
			get
			{
				return WeaverAssets.LoadWeaverAsset<AudioClip>("Boss Explosion");
			}
		}
		public static AudioClip BossExplosionUninfected
		{
			get
			{
				return WeaverAssets.LoadWeaverAsset<AudioClip>("Boss Explosion Uninfected");
			}
		}
	}
}
