using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Assets;

namespace WeaverCore.Utilities
{
	public static class Flings
	{
		static ObjectPool GhostSlash1Pool;
		static ObjectPool GhostSlash2Pool;

		static GameObject[] SpawnFlingsInternal(FlingInfo info, Vector3 spawnPoint, CardinalDirection direction)
		{
			float angleMin = info.AngleMin;
			float angleMax = info.AngleMax;

			AdjustAnglesForDirection(ref angleMin, ref angleMax, direction);

			int prefabAmount = UnityEngine.Random.Range(info.PrefabAmountMin, info.PrefabAmountMax + 1);
			//List<GameObject> allFlings = new List<GameObject>();
			GameObject[] allFlings = new GameObject[prefabAmount];
			for (int i = 0; i < prefabAmount; i++)
			{
				Vector3 finalPosition = spawnPoint + new Vector3(UnityEngine.Random.Range(-info.OriginVariationX, info.OriginVariationX), UnityEngine.Random.Range(-info.OriginVariationY, info.OriginVariationY), 0f);
				//GameObject newFling = GameObject.Instantiate(info.Prefab, finalPosition,Quaternion.identity);
				GameObject newFling = info.Pool.Instantiate(finalPosition, Quaternion.identity);
				Rigidbody2D rigidbody = newFling.GetComponent<Rigidbody2D>();
				if (rigidbody != null)
				{
					float velocity = UnityEngine.Random.Range(info.VelocityMin, info.VelocityMax);
					float angle = UnityEngine.Random.Range(angleMin, angleMax);

					var eulerAngles = newFling.transform.eulerAngles;
					eulerAngles.z = angle;
					newFling.transform.eulerAngles = eulerAngles;

					rigidbody.velocity = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * velocity;
				}
				//allFlings.Add(newFling);
				allFlings[i] = newFling;
			}
			return allFlings;
		}

		static void AdjustAnglesForDirection(ref float angleMin, ref float angleMax,CardinalDirection direction)
		{
			switch (direction)
			{
				case CardinalDirection.Up:
					angleMin += 90f;
					angleMax += 90f;
					break;
				case CardinalDirection.Down:
					angleMin += 270f;
					angleMax += 270f;
					break;
				case CardinalDirection.Left:
					angleMin += 180f;
					angleMax += 180f;
					break;
				default:
					break;
			}
		}

		public static FlingInfo[] CreateNormalFlings()
		{
			if (GhostSlash1Pool == null)
			{
				GhostSlash1Pool = ObjectPool.Create(EffectAssets.GhostSlash1Prefab);
				GhostSlash1Pool.FillPool(1);

				GhostSlash2Pool = ObjectPool.Create(EffectAssets.GhostSlash2Prefab);
				GhostSlash2Pool.FillPool(1);
			}

			return new FlingInfo[2]
			{
				new FlingInfo
				{
					Pool = GhostSlash1Pool,
					PrefabAmountMin = 2,
					PrefabAmountMax = 3,
					VelocityMin = 20f,
					VelocityMax = 35f,
					AngleMin = -40f,
					AngleMax = 40f,
					OriginVariationX = 0f,
					OriginVariationY = 0f
				},
				new FlingInfo
				{
					Pool = GhostSlash2Pool,
					PrefabAmountMin = 2,
					PrefabAmountMax = 3,
					VelocityMin = 10f,
					VelocityMax = 35f,
					AngleMin = -40f,
					AngleMax = 40f,
					OriginVariationX = 0f,
					OriginVariationY = 0f
				}
			};

			/*var settings1 = new FlingInfo()
			{
				Pool = GhostSlash1Pool,
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
				Pool = GhostSlash2Pool,
				PrefabAmountMin = 2,
				PrefabAmountMax = 3,
				VelocityMin = 10f,
				VelocityMax = 35f,
				AngleMin = -40f,
				AngleMax = 40f,
				OriginVariationX = 0f,
				OriginVariationY = 0f
			};*/
		}

		public static void SpawnFlings(FlingInfo[] flings, Vector3 spawnPoint, CardinalDirection direction)
		{
			//IEnumerable<GameObject> iterator = Enumerable.Empty<GameObject>();

			for (int i = 0; i < flings.Length; i++)
			{
				SpawnFlingsInternal(flings[i], spawnPoint, direction);
				//var finalObjects = SpawnFlingsInternal(settings1, spawnPoint);
				//finalObjects.AddRange(SpawnFlingsInternal(settings2, spawnPoint));
			}
		}

		public static GameObject[] SpawnFlings(FlingInfo fling, Vector3 spawnPoint, CardinalDirection direction)
		{
			return SpawnFlingsInternal(fling, spawnPoint, direction);
		}
	}


	public struct FlingInfo
	{
		public ObjectPool Pool;

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
