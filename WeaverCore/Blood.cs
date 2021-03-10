using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore
{
	public class Blood : MonoBehaviour
	{
		//static ObjectPool BloodPool;
		static GameObject bloodPrefab;

		public const float SpeedMultiplier = 1.2f;
		public const float AmountMultiplier = 1.3f;


		public struct BloodSpawnInfo
		{
			public int MinCount;
			public int MaxCount;
			public float MinSpeed;
			public float MaxSpeed;
			public float AngleMin;
			public float AngleMax;
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


		public static Blood SpawnBlood(Vector3 position, BloodSpawnInfo spawnInfo)
		{
			if (bloodPrefab == null)
			{
				bloodPrefab = WeaverAssets.LoadWeaverAsset<GameObject>("Blood Particles");
			}
			/*if (BloodPool == null)
			{
				BloodPool = new ObjectPool(WeaverAssets.LoadWeaverAsset<GameObject>("Blood Particles"));
			}*/

			//var instance = BloodPool.Instantiate(position, Quaternion.identity);
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

		/*[SerializeField]
		short bloodSpawnMin = 12;
		[SerializeField]
		short bloodSpawnMax = 15;
		[SerializeField]
		float bloodSpeedMin = 24f;
		[SerializeField]
		float bloodSpeedMax = 30f;
		[SerializeField]
		float bloodAngleMin = 0f;
		[SerializeField]
		float bloodAngleMax = 360f;
		[SerializeField]
		float bloodSpawnDelay = 0.1f;
		[SerializeField]
		Vector3 bloodSpawnOffset = new Vector3(0f, 0f, 0.002f);
		[SerializeField]
		GameObject bloodSplatterParticle;
		[SerializeField]
		float bloodSpeedMultiplier = 1.2f;
		[SerializeField]
		float bloodAmountMultiplier = 1.3f;*/

		public static Blood SpawnRandomBlood(Vector3 position)
		{
			return SpawnBlood(position + new Vector3(0f, 0f, 0.002f), new BloodSpawnInfo(12,15,24f,30f,0f,360f));
		}

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
