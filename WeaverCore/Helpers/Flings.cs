using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.WeaverAssets;

namespace WeaverCore.Helpers
{
	public static class Flings
	{
		public static List<GameObject> SpawnFlings(FlingInfo info, Vector3 spawnPoint)
		{
			if (info.Prefab == null)
			{
				return null;
			}
			int prefabAmount = UnityEngine.Random.Range(info.PrefabAmountMin, info.PrefabAmountMax + 1);
			List<GameObject> allFlings = new List<GameObject>();
			for (int i = 0; i < prefabAmount; i++)
			{
				Vector3 finalPosition = spawnPoint + new Vector3(UnityEngine.Random.Range(-info.OriginVariationX, info.OriginVariationX), UnityEngine.Random.Range(-info.OriginVariationY, info.OriginVariationY), 0f);
				GameObject newFling = GameObject.Instantiate(info.Prefab, finalPosition,Quaternion.identity);
				Rigidbody2D rigidbody = newFling.GetComponent<Rigidbody2D>();
				if (rigidbody != null)
				{
					float velocity = UnityEngine.Random.Range(info.VelocityMin, info.VelocityMax);
					float angle = UnityEngine.Random.Range(info.AngleMin, info.AngleMax);

					var eulerAngles = newFling.transform.eulerAngles;
					eulerAngles.z = angle;
					newFling.transform.eulerAngles = eulerAngles;

					rigidbody.velocity = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * velocity;
				}
				allFlings.Add(newFling);
			}
			return allFlings;
		}


		public static List<GameObject> SpawnFlingsNormal(Vector3 spawnPoint, CardinalDirection direction)
		{
			var settings1 = new FlingInfo()
			{
				Prefab = EffectAssets.GhostSlash1Prefab,
				PrefabAmountMin = 2,
				PrefabAmountMax = 3,
				VelocityMin = 20f,
				VelocityMax = 35f,
				AngleMin = -40f,
				AngleMax = 40f,
				OriginVariationX = 0f,
				OriginVariationY = 0f
			};

			var settings2 = new FlingInfo()
			{
				Prefab = EffectAssets.GhostSlash2Prefab,
				PrefabAmountMin = 2,
				PrefabAmountMax = 3,
				VelocityMin = 10f,
				VelocityMax = 35f,
				AngleMin = -40f,
				AngleMax = 40f,
				OriginVariationX = 0f,
				OriginVariationY = 0f
			};

			switch (direction)
			{
				case CardinalDirection.Up:
					settings1.AngleMin = 50f;
					settings1.AngleMax = 130f;

					settings2.AngleMin = 50f;
					settings2.AngleMax = 130f;
					break;
				case CardinalDirection.Down:
					settings1.AngleMin = 230f;
					settings1.AngleMax = 310f;

					settings2.AngleMin = 230f;
					settings2.AngleMax = 310f;
					break;
				case CardinalDirection.Left:
					settings1.AngleMin = 140f;
					settings1.AngleMax = 220f;

					settings2.AngleMin = 140f;
					settings2.AngleMax = 220f;
					break;
				case CardinalDirection.Right:
					settings1.AngleMin = -40f;
					settings1.AngleMax = 40f;

					settings2.AngleMin = -40f;
					settings2.AngleMax = 40f;
					break;
				default:
					break;
			}

			var finalObjects = SpawnFlings(settings1, spawnPoint);
			finalObjects.AddRange(SpawnFlings(settings2, spawnPoint));

			return finalObjects;
		}
	}


	[Serializable]
	public struct FlingInfo
	{
		public GameObject Prefab;

		public float VelocityMin;

		public float VelocityMax;

		public float AngleMin;

		public float AngleMax;

		public float OriginVariationX;

		public float OriginVariationY;

		public int PrefabAmountMin;

		public int PrefabAmountMax;
	}
}
