using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FlingUtils;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Provides extra functions and utilities for flinging objects
    /// </summary>
    public static class FlingUtilities
    {

        /// <summary>
        /// Flings a spawned object with the specified speed and angle.
        /// </summary>
        /// <param name="speedRange">The range of speeds for flinging.</param>
        /// <param name="angleRange">The range of angles for flinging.</param>
        /// <param name="spawnedObj">The GameObject to be flung.</param>
        public static void FlingSpawnedObject(Vector2 speedRange, Vector2 angleRange, GameObject spawnedObj)
        {
            float speed = speedRange.RandomInRange();
            float angle = angleRange.RandomInRange();

            Vector2 velocity = new Vector2(speed * Mathf.Cos(angle * (Mathf.PI / 180f)), speed * Mathf.Sin(angle * (Mathf.PI / 180f)));

            var rb2d = spawnedObj.GetComponent<Rigidbody2D>();
            rb2d.velocity = velocity;
        }

        /// <summary>
        /// Spawns a specified number of pooled objects and flings them based on the provided configuration.
        /// </summary>
        /// <param name="config">The configuration for spawning and flinging.</param>
        /// <param name="spawnPoint">The transform representing the spawn point.</param>
        /// <param name="positionOffset">The offset from the spawn point position.</param>
        /// <returns>An array of GameObjects spawned and flung.</returns>
        public static GameObject[] SpawnPooledAndFling(Config config, Transform spawnPoint, Vector3 positionOffset)
        {
            if (config.Prefab == null)
            {
                return null;
            }

            int num = UnityEngine.Random.Range(config.AmountMin, config.AmountMax + 1);
            Vector3 vector = ((spawnPoint != null) ? spawnPoint.TransformPoint(positionOffset) : positionOffset);
            GameObject[] array = new GameObject[num];
            for (int i = 0; i < num; i++)
            {
                Vector3 position = vector + new Vector3(UnityEngine.Random.Range(0f - config.OriginVariationX, config.OriginVariationX), UnityEngine.Random.Range(0f - config.OriginVariationY, config.OriginVariationY), 0f);
                GameObject gameObject = Pooling.Instantiate(config.Prefab, position, Quaternion.identity);
                Rigidbody2D component = gameObject.GetComponent<Rigidbody2D>();
                if (component != null)
                {
                    float num2 = UnityEngine.Random.Range(config.SpeedMin, config.SpeedMax);
                    float num3 = UnityEngine.Random.Range(config.AngleMin, config.AngleMax);
                    component.velocity = new Vector2(Mathf.Cos(num3 * ((float)Math.PI / 180f)), Mathf.Sin(num3 * ((float)Math.PI / 180f))) * num2;
                }
                array[i] = gameObject;
            }
            return array;
        }

        /// <summary>
        /// Spawns a specified number of random objects and flings them based on the provided parameters.
        /// </summary>
        /// <param name="prefab">The GameObject prefab to spawn.</param>
        /// <param name="spawnPos">The spawn position for the objects.</param>
        /// <param name="spawnAmountRange">The range for the number of objects to spawn.</param>
        /// <param name="speedRange">The range of speeds for flinging.</param>
        /// <param name="angleRange">The range of angles for flinging.</param>
        /// <param name="originVariation">The variation in origin positions.</param>
        /// <returns>An array of spawned and flung GameObjects.</returns>
        public static GameObject[] SpawnRandomObjectsPooled(GameObject prefab, Vector3 spawnPos, Vector2Int spawnAmountRange, Vector2 speedRange, Vector2 angleRange, Vector2 originVariation)
        {
            var spawnAmount = UnityEngine.Random.Range(spawnAmountRange.x, spawnAmountRange.y + 1);

            GameObject[] objs = new GameObject[spawnAmount];

            for (int i = 1; i <= spawnAmount; i++)
            {
                var originVariationX = UnityEngine.Random.Range(-originVariation.x, originVariation.x);
                var originVariationY = UnityEngine.Random.Range(-originVariation.y, originVariation.y);

                var spatter = Pooling.Instantiate(prefab, spawnPos, Quaternion.identity);
                spatter.transform.position += new Vector3(originVariationX, originVariationY);

                var spatterRB = spatter.GetComponent<Rigidbody2D>();

                if (spatterRB != null)
                {
                    float speed = speedRange.RandomInRange();
                    float angle = angleRange.RandomInRange();

                    spatterRB.velocity = MathUtilities.PolarToCartesian(angle, speed);
                }

                objs[i - 1] = spatter;
            }

            return objs;
        }

        /// <summary>
        /// Spawns a specified number of random objects and flings them based on the provided parameters.
        /// </summary>
        /// <param name="prefab">The GameObject prefab to spawn.</param>
        /// <param name="spawnPos">The spawn position for the objects.</param>
        /// <param name="spawnAmountRange">The range for the number of objects to spawn.</param>
        /// <param name="speedRange">The range of speeds for flinging.</param>
        /// <param name="angleRange">The range of angles for flinging.</param>
        /// <param name="originVariation">The variation in origin positions.</param>
        /// <returns>An array of spawned and flung GameObjects.</returns>
        public static GameObject[] SpawnRandomObjects(GameObject prefab, Vector3 spawnPos, Vector2Int spawnAmountRange, Vector2 speedRange, Vector2 angleRange, Vector2 originVariation)
        {
            var spawnAmount = UnityEngine.Random.Range(spawnAmountRange.x, spawnAmountRange.y + 1);

            GameObject[] objs = new GameObject[spawnAmount];

            for (int i = 1; i <= spawnAmount; i++)
            {
                var originVariationX = UnityEngine.Random.Range(-originVariation.x, originVariation.x);
                var originVariationY = UnityEngine.Random.Range(-originVariation.y, originVariation.y);

                var spatter = GameObject.Instantiate(prefab, spawnPos, Quaternion.identity);
                spatter.transform.position += new Vector3(originVariationX, originVariationY);

                var spatterRB = spatter.GetComponent<Rigidbody2D>();

                if (spatterRB != null)
                {
                    float speed = speedRange.RandomInRange();
                    float angle = angleRange.RandomInRange();

                    spatterRB.velocity = MathUtilities.PolarToCartesian(speed, angle);
                }

                objs[i - 1] = spatter;
            }

            return objs;
        }

        /// <summary>
        /// Spawns random objects over a specified duration with a given frequency, and flings them based on the provided parameters.
        /// </summary>
        /// <param name="prefab">The GameObject prefab to spawn.</param>
        /// <param name="duration">The total duration for spawning objects.</param>
        /// <param name="frequency">The frequency of spawning objects.</param>
        /// <param name="spawnPos">The spawn position for the objects.</param>
        /// <param name="spawnAmountRange">The range for the number of objects to spawn.</param>
        /// <param name="speedRange">The range of speeds for flinging.</param>
        /// <param name="angleRange">The range of angles for flinging.</param>
        /// <param name="originVariation">The variation in origin positions.</param>
        /// <param name="scaleRange">The range of scales for the spawned objects.</param>
        /// <returns>An IEnumerator for coroutine usage.</returns>
        public static IEnumerator SpawnRandomObjectsOverTime(GameObject prefab, float duration, float frequency, Vector3 spawnPos, Vector2Int spawnAmountRange, Vector2 speedRange, Vector2 angleRange, Vector2 originVariation, Vector2 scaleRange)
        {
            return SpawnRandomObjectsOverTime(prefab, duration, frequency, () => spawnPos, spawnAmountRange, speedRange, angleRange, originVariation, scaleRange);
        }

        /// <summary>
        /// Spawns random objects over a specified duration with a given frequency, and flings them based on the provided parameters.
        /// </summary>
        /// <param name="prefab">The GameObject prefab to spawn.</param>
        /// <param name="duration">The total duration for spawning objects.</param>
        /// <param name="frequency">The frequency of spawning objects.</param>
        /// <param name="spawnPos">The spawn position for the objects.</param>
        /// <param name="spawnAmountRange">The range for the number of objects to spawn.</param>
        /// <param name="speedRange">The range of speeds for flinging.</param>
        /// <param name="angleRange">The range of angles for flinging.</param>
        /// <param name="originVariation">The variation in origin positions.</param>
        /// <param name="scaleRange">The range of scales for the spawned objects.</param>
        /// <returns>An IEnumerator for coroutine usage.</returns>
        public static IEnumerator SpawnRandomObjectsOverTime(GameObject prefab, float duration, float frequency, Func<Vector3> spawnPos, Vector2Int spawnAmountRange, Vector2 speedRange, Vector2 angleRange, Vector2 originVariation, Vector2 scaleRange)
        {
            float timer = 0f;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                yield return null;
                timer += Time.deltaTime;
                if (timer >= frequency)
                {
                    timer -= frequency;
                    foreach (var obj in SpawnRandomObjects(prefab, spawnPos(), spawnAmountRange, speedRange, angleRange, originVariation))
                    {
                        var scale = scaleRange.RandomInRange();
                        obj.transform.localScale = new Vector3(scale, scale, scale);
                    }
                }
            }
        }

        /// <summary>
        /// Spawns random pooled objects over a specified duration with a given frequency, and flings them based on the provided parameters.
        /// </summary>
        /// <param name="prefab">The GameObject prefab to spawn.</param>
        /// <param name="duration">The total duration for spawning objects.</param>
        /// <param name="frequency">The frequency of spawning objects.</param>
        /// <param name="spawnPos">The spawn position for the objects.</param>
        /// <param name="spawnAmountRange">The range for the number of objects to spawn.</param>
        /// <param name="speedRange">The range of speeds for flinging.</param>
        /// <param name="angleRange">The range of angles for flinging.</param>
        /// <param name="originVariation">The variation in origin positions.</param>
        /// <param name="scaleRange">The range of scales for the spawned objects.</param>
        /// <returns>An IEnumerator for coroutine usage.</returns>
        public static IEnumerator SpawnRandomObjectsPooledOverTime(GameObject prefab, float duration, float frequency, Vector3 spawnPos, Vector2Int spawnAmountRange, Vector2 speedRange, Vector2 angleRange, Vector2 originVariation, Vector2 scaleRange)
        {
            return SpawnRandomObjectsPooledOverTime(prefab, duration, frequency, () => spawnPos, spawnAmountRange, speedRange, angleRange, originVariation, scaleRange);
        }

        /// <summary>
        /// Spawns random pooled objects over a specified duration with a given frequency, and flings them based on the provided parameters.
        /// </summary>
        /// <param name="prefab">The GameObject prefab to spawn.</param>
        /// <param name="duration">The total duration for spawning objects.</param>
        /// <param name="frequency">The frequency of spawning objects.</param>
        /// <param name="spawnPos">The spawn position for the objects.</param>
        /// <param name="spawnAmountRange">The range for the number of objects to spawn.</param>
        /// <param name="speedRange">The range of speeds for flinging.</param>
        /// <param name="angleRange">The range of angles for flinging.</param>
        /// <param name="originVariation">The variation in origin positions.</param>
        /// <param name="scaleRange">The range of scales for the spawned objects.</param>
        /// <returns>An IEnumerator for coroutine usage.</returns>
        public static IEnumerator SpawnRandomObjectsPooledOverTime(GameObject prefab, float duration, float frequency, Func<Vector3> spawnPos, Vector2Int spawnAmountRange, Vector2 speedRange, Vector2 angleRange, Vector2 originVariation, Vector2 scaleRange)
        {
            float timer = 0f;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                yield return null;
                timer += Time.deltaTime;
                if (timer >= frequency)
                {
                    timer -= frequency;
                    foreach (var obj in SpawnRandomObjectsPooled(prefab, spawnPos(), spawnAmountRange, speedRange, angleRange, originVariation))
                    {
                        var scale = scaleRange.RandomInRange();
                        obj.transform.localScale = new Vector3(scale, scale, scale);
                    }
                }
            }
        }
    }
}
