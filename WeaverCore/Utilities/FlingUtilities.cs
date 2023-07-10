using System;
using UnityEngine;
using static FlingUtils;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Provides extra functions and utilities for flinging objects
    /// </summary>
    public static class FlingUtilities
	{
        public static void FlingSpawnedObject(Vector2 speedRange, Vector2 angleRange, GameObject spawnedObj)
        {
            float speed = speedRange.RandomInRange();
            float angle = angleRange.RandomInRange();

            Vector2 velocity = new Vector2(speed * Mathf.Cos(angle * (Mathf.PI / 180f)), speed * Mathf.Sin(angle * (Mathf.PI / 180f)));

            var rb2d = spawnedObj.GetComponent<Rigidbody2D>();
            rb2d.velocity = velocity;
        }

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

    }
}
