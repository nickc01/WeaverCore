using System;
using UnityEngine;

public static class FlingUtils
{
    [Serializable]
    public struct Config
    {
        public GameObject Prefab;

        public float SpeedMin;

        public float SpeedMax;

        public float AngleMin;

        public float AngleMax;

        public float OriginVariationX;

        public float OriginVariationY;

        public int AmountMin;

        public int AmountMax;
    }

    public struct ChildrenConfig
    {
        public GameObject Parent;

        public int AmountMin;

        public int AmountMax;

        public float SpeedMin;

        public float SpeedMax;

        public float AngleMin;

        public float AngleMax;

        public float OriginVariationX;

        public float OriginVariationY;
    }

    public struct SelfConfig
    {
        public GameObject Object;

        public float SpeedMin;

        public float SpeedMax;

        public float AngleMin;

        public float AngleMax;
    }

    /*static GameObject DefaultSpawnFunction(GameObject prefab, Vector3 position)
    {
        return GameObject.Instantiate(prefab, position, Quaternion.identity);
    }*/

    public static GameObject[] SpawnAndFling(Config config, Transform spawnPoint, Vector3 positionOffset)
    {
        if (config.Prefab == null)
        {
            return null;
        }

        /*if (spawnPrefabFunc == null)
        {
            spawnPrefabFunc = DefaultSpawnFunction;
        }*/

        int num = UnityEngine.Random.Range(config.AmountMin, config.AmountMax + 1);
        Vector3 vector = ((spawnPoint != null) ? spawnPoint.TransformPoint(positionOffset) : positionOffset);
        GameObject[] array = new GameObject[num];
        for (int i = 0; i < num; i++)
        {
            Vector3 position = vector + new Vector3(UnityEngine.Random.Range(0f - config.OriginVariationX, config.OriginVariationX), UnityEngine.Random.Range(0f - config.OriginVariationY, config.OriginVariationY), 0f);
            //GameObject gameObject = config.Prefab.Spawn(position);
            GameObject gameObject = GameObject.Instantiate(config.Prefab, position, Quaternion.identity);
            //GameObject gameObject = spawnPrefabFunc(config.Prefab, position);
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

    public static void FlingChildren(ChildrenConfig config, Transform spawnPoint, Vector3 positionOffset)
    {
        if (config.Parent == null)
        {
            return;
        }
        Vector3 vector = ((spawnPoint != null) ? spawnPoint.TransformPoint(positionOffset) : positionOffset);
        int num = ((config.AmountMax > 0) ? UnityEngine.Random.Range(config.AmountMin, config.AmountMax) : config.Parent.transform.childCount);
        for (int i = 0; i < num; i++)
        {
            Transform child = config.Parent.transform.GetChild(i);
            child.gameObject.SetActive(value: true);
            _ = vector + new Vector3(UnityEngine.Random.Range(0f - config.OriginVariationX, config.OriginVariationX), UnityEngine.Random.Range(0f - config.OriginVariationY, config.OriginVariationY), 0f);
            Rigidbody2D component = child.GetComponent<Rigidbody2D>();
            if (component != null)
            {
                float num2 = UnityEngine.Random.Range(config.SpeedMin, config.SpeedMax);
                float num3 = UnityEngine.Random.Range(config.AngleMin, config.AngleMax);
                component.velocity = new Vector2(Mathf.Cos(num3 * ((float)Math.PI / 180f)), Mathf.Sin(num3 * ((float)Math.PI / 180f))) * num2;
            }
        }
    }

    public static void FlingObject(SelfConfig config, Transform spawnPoint, Vector3 positionOffset)
    {
        if (!(config.Object == null))
        {
            if (spawnPoint != null)
            {
                spawnPoint.TransformPoint(positionOffset);
            }
            Rigidbody2D component = config.Object.GetComponent<Rigidbody2D>();
            if (component != null)
            {
                float num = UnityEngine.Random.Range(config.SpeedMin, config.SpeedMax);
                float num2 = UnityEngine.Random.Range(config.AngleMin, config.AngleMax);
                component.velocity = new Vector2(Mathf.Cos(num2 * ((float)Math.PI / 180f)), Mathf.Sin(num2 * ((float)Math.PI / 180f))) * num;
            }
        }
    }
}
