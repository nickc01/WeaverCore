using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Assets;

namespace WeaverCore
{
    /// <summary>
    /// Used for creating "Flings" that will scatter particle objects in random directions
    /// </summary>
    public static class Flings
    {
        //static ObjectPool GhostSlash1Pool;
        //static ObjectPool GhostSlash2Pool;

        static GameObject[] SpawnFlingsInternal(FlingInfo info, Vector3 spawnPoint, CardinalDirection direction)
        {
            float angleMin = info.AngleMin;
            float angleMax = info.AngleMax;

            AdjustAnglesForDirection(ref angleMin, ref angleMax, direction);

            int prefabAmount = UnityEngine.Random.Range(info.PrefabAmountMin, info.PrefabAmountMax + 1);
            GameObject[] allFlings = new GameObject[prefabAmount];
            for (int i = 0; i < prefabAmount; i++)
            {
                Vector3 finalPosition = spawnPoint + new Vector3(UnityEngine.Random.Range(-info.OriginVariationX, info.OriginVariationX), UnityEngine.Random.Range(-info.OriginVariationY, info.OriginVariationY), 0f);
                //GameObject newFling = info.Pool.Instantiate(finalPosition, Quaternion.identity);
                GameObject newFling = Pooling.Instantiate(info.Prefab, finalPosition, Quaternion.identity);
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
                allFlings[i] = newFling;
            }
            return allFlings;
        }

        static void AdjustAnglesForDirection(ref float angleMin, ref float angleMax, CardinalDirection direction)
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

        /// <summary>
        /// Creates some fling particles that are used as the hit effects for normal enemies
        /// </summary>
        /// <returns>Returns the fling info needed to spawn the flings</returns>
        public static FlingInfo[] CreateNormalFlings()
        {
            /*if (GhostSlash1Pool == null)
            {
                GhostSlash1Pool = ObjectPool.Create(EffectAssets.GhostSlash1Prefab);
                GhostSlash1Pool.FillPool(1);

                GhostSlash2Pool = ObjectPool.Create(EffectAssets.GhostSlash2Prefab);
                GhostSlash2Pool.FillPool(1);
            }*/

            return new FlingInfo[2]
            {
                new FlingInfo
                {
                    Prefab = EffectAssets.SlashGhost1Prefab,
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
                    Prefab = EffectAssets.SlashGhost2Prefab,
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
        }

        /// <summary>
        /// Spawns the flings
        /// </summary>
        /// <param name="flings">The flings to spawn</param>
        /// <param name="spawnPoint">The spawn point where the fling effects will originate from</param>
        /// <param name="direction">The direction the flings should travel in</param>
        public static void SpawnFlings(FlingInfo[] flings, Vector3 spawnPoint, CardinalDirection direction)
        {
            for (int i = 0; i < flings.Length; i++)
            {
                SpawnFlingsInternal(flings[i], spawnPoint, direction);
            }
        }

        /// <summary>
        /// Spawns the flings
        /// </summary>
        /// <param name="flings">The flings to spawn</param>
        /// <param name="spawnPoint">The spawn point where the fling effects will originate from</param>
        public static void SpawnFlings(FlingInfo[] flings, Vector3 spawnPoint)
        {
            SpawnFlings(flings, spawnPoint, CardinalDirection.Right);
        }

        /// <summary>
        /// Spawns the flings
        /// </summary>
        /// <param name="fling">The flings to spawn</param>
        /// <param name="spawnPoint">The spawn point where the fling effects will originate from</param>
        /// <param name="direction">The direction the flings should travel in</param>
        /// <returns>Returns the fling objects that have been created</returns>
        public static GameObject[] SpawnFlings(FlingInfo fling, Vector3 spawnPoint, CardinalDirection direction)
        {
            return SpawnFlingsInternal(fling, spawnPoint, direction);
        }

        /// <summary>
        /// Spawns the flings
        /// </summary>
        /// <param name="fling">The flings to spawn</param>
        /// <param name="spawnPoint">The spawn point where the fling effects will originate from</param>
        /// <returns>Returns the fling objects that have been created</returns>
        public static GameObject[] SpawnFlings(FlingInfo fling, Vector3 spawnPoint)
        {
            return SpawnFlings(fling, spawnPoint, CardinalDirection.Right);
        }
    }

    /// <summary>
    /// Contains all the needed information for spawning a fling effect
    /// </summary>
    public struct FlingInfo
    {
        /// <summary>
        /// The prefab to spawn
        /// </summary>
        public GameObject Prefab;

        /// <summary>
        /// The minimum spawn velocity of the fling
        /// </summary>
        public float VelocityMin;

        /// <summary>
        /// The maximum spawn velocity of a fling
        /// </summary>
        public float VelocityMax;

        /// <summary>
        /// The minimum spawn angle of a fling
        /// </summary>
        public float AngleMin;

        /// <summary>
        /// The maximum spawn angle of a fling
        /// </summary>
        public float AngleMax;

        /// <summary>
        /// The origin variation of a fling along the x-axis
        /// </summary>
        public float OriginVariationX;

        /// <summary>
        /// The origin variation of a fling along the y-axis
        /// </summary>
        public float OriginVariationY;

        /// <summary>
        /// The minimum amount of prefabs to spawn for a fling
        /// </summary>
        public int PrefabAmountMin;

        /// <summary>
        /// The maximum amount of prefabs to spawn for a fling
        /// </summary>
        public int PrefabAmountMax;
    }
}
