using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore
{

    /// <summary>
    /// Used for spawning blood particle effects
    /// </summary>
    public class Blood : MonoBehaviour
	{
		static GameObject bloodPrefab;

		public const float SpeedMultiplier = 1.2f;
		public const float AmountMultiplier = 1.3f;

		/// <summary>
		/// A struct that details how blood particles should be spawned
		/// </summary>
		public struct BloodSpawnInfo
		{
			/// <summary>
			/// The min amount of blood particles to spawn
			/// </summary>
			public int MinCount;

			/// <summary>
			/// The max amount of blood particles to spawn
			/// </summary>
			public int MaxCount;

			/// <summary>
			/// The min velocity of the blood particles
			/// </summary>
			public float MinSpeed;

			/// <summary>
			/// The max velocity of the blood particles
			/// </summary>
			public float MaxSpeed;

			/// <summary>
			/// The minimum angle direction of the blood particles
			/// </summary>
			public float AngleMin;

			/// <summary>
			/// The maximum angle direction of the blood particles
			/// </summary>
			public float AngleMax;

			/// <summary>
			/// The color of the blood particles. If left null, will use the default color
			/// </summary>
			public Color? ColorOverride;

			public BloodSpawnInfo(	int minCount, int maxCount, float minSpeed, float maxSpeed,
									float angleMin, float angleMax, Color? colorOverride = null)
			{
				MinCount = minCount;
				MaxCount = maxCount;
				MinSpeed = minSpeed;
				MaxSpeed = maxSpeed;
				AngleMin = angleMin;
				AngleMax = angleMax;
				ColorOverride = colorOverride;
			}
		}


		/// <summary>
		/// Spawns blood particles
		/// </summary>
		/// <param name="position">The position to spawn the particles</param>
		/// <param name="spawnInfo">The info object used to determine how the blood is spawned</param>
		/// <returns>Returns an instance to the blood particle system creating the effects</returns>
		public static Blood SpawnBlood(Vector3 position, BloodSpawnInfo spawnInfo)
		{
			if (bloodPrefab == null)
			{
				bloodPrefab = WeaverAssets.LoadWeaverAsset<GameObject>("Blood Particles");
			}

			var instance = Pooling.Instantiate(bloodPrefab, position, Quaternion.identity);
			var particleSystem = instance.GetComponent<ParticleSystem>();
			particleSystem.Stop();
			particleSystem.emission.SetBursts(new ParticleSystem.Burst[]
			{
				new ParticleSystem.Burst(0f,(short)Mathf.RoundToInt(spawnInfo.MinCount * AmountMultiplier),(short)Mathf.RoundToInt(spawnInfo.MaxCount * AmountMultiplier))
			});
			var main = particleSystem.main;

			main.maxParticles = Mathf.RoundToInt(spawnInfo.MaxCount * AmountMultiplier);
			main.startSpeed = new ParticleSystem.MinMaxCurve(spawnInfo.MinSpeed * SpeedMultiplier, spawnInfo.MaxSpeed * SpeedMultiplier);
			if (spawnInfo.ColorOverride != null)
			{
				main.startColor = new ParticleSystem.MinMaxGradient(spawnInfo.ColorOverride.Value);
			}

			var shape = particleSystem.shape;
			shape.arc = spawnInfo.AngleMax - spawnInfo.AngleMin;
			instance.transform.SetZRotation(spawnInfo.AngleMin);
			particleSystem.Play();

			return instance.GetComponent<Blood>();
		}

		/// <summary>
		/// Spawns a bunch of blood particles in random directions
		/// </summary>
		/// <param name="position">The position the blood should be spawned at</param>
		/// <returns>Returns an instance to the blood particle system creating the effects</returns>
		public static Blood SpawnRandomBlood(Vector3 position)
		{
			return SpawnBlood(position + new Vector3(0f, 0f, 0.002f), new BloodSpawnInfo(12,15,24f,30f,0f,360f));
		}

		/// <summary>
		/// Spawns blood effects that travel in a certain direction
		/// </summary>
		/// <param name="position">The position the blood should be spawned at</param>
		/// <param name="direction">The general direction the blood should be traveling in</param>
		/// <returns>Returns a list of all the blood particle systems creating the effects</returns>
		public static IEnumerable<Blood> SpawnDirectionalBlood(Vector3 position, CardinalDirection direction)
		{
			Blood[] bloods = null;
			switch (direction)
			{
				case CardinalDirection.Up:
					bloods = new Blood[1];
					bloods[0] = SpawnBlood(position, new BloodSpawnInfo(8, 10, 20f, 30f, 80f, 100f, null));
					return bloods;
				case CardinalDirection.Down:
					bloods = new Blood[2];
					bloods[0] = SpawnBlood(position, new BloodSpawnInfo(4, 5, 15f, 25f, 140f, 180f, null));
					bloods[1] = SpawnBlood(position, new BloodSpawnInfo(4, 5, 15f, 25f, 360f, 400f, null));
					return bloods;
				case CardinalDirection.Left:
					bloods = new Blood[2];
					bloods[0] = SpawnBlood(position, new BloodSpawnInfo(3, 4, 10f, 15f, 30f, 60f, null));
					bloods[1] = SpawnBlood(position, new BloodSpawnInfo(8, 10, 15f, 25f, 120f, 150f, null));
					return bloods;
				default: //RIGHT
					bloods = new Blood[2];
					bloods[0] = SpawnBlood(position, new BloodSpawnInfo(3, 4, 10f, 15f, 120f, 150f, null));
					bloods[1] = SpawnBlood(position, new BloodSpawnInfo(8, 15, 10f, 25f, 30f, 60f, null));
					return bloods;
			}
		}

		ParticleSystem _particles;

		/// <summary>
		/// The particle system emitting the blood effects
		/// </summary>
		public ParticleSystem Particles
		{
			get
			{
				if (_particles == null)
				{
					_particles = GetComponent<ParticleSystem>();
				}
				return _particles;
			}
		}
	}
}
