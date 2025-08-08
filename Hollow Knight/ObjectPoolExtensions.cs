using System.Collections.Generic;
using UnityEngine;

public static class ObjectPoolExtensions
{
    static T SpawnInternal<T>(T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
    {
        var instance = GameObject.Instantiate(prefab, position, rotation);
        instance.transform.SetParent(parent);
        return instance;
    }

    static GameObject SpawnInternal(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
    {
        var instance = GameObject.Instantiate(prefab, position, rotation);
        instance.transform.SetParent(parent);
        return instance;
    }

	public static T Spawn<T>(this T prefab, Transform parent, Vector3 position, Quaternion rotation) where T : Component
    {
        return SpawnInternal(prefab, parent, position, rotation);
    }

	public static T Spawn<T>(this T prefab, Vector3 position, Quaternion rotation) where T : Component
	{
		return SpawnInternal(prefab, null, position, rotation);
	}

	public static T Spawn<T>(this T prefab, Transform parent, Vector3 position) where T : Component
	{
		return SpawnInternal(prefab, parent, position, Quaternion.identity);
	}

	public static T Spawn<T>(this T prefab, Vector3 position) where T : Component
	{
		return SpawnInternal(prefab, null, position, Quaternion.identity);
	}

	public static T Spawn<T>(this T prefab, Transform parent) where T : Component
	{
		return SpawnInternal(prefab, parent, Vector3.zero, Quaternion.identity);
	}

	public static T Spawn<T>(this T prefab) where T : Component
	{
		return SpawnInternal(prefab, null, Vector3.zero, Quaternion.identity);
	}

	public static GameObject Spawn(this GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
	{
		return SpawnInternal(prefab, parent, position, rotation);
	}

	public static GameObject Spawn(this GameObject prefab, Vector3 position, Quaternion rotation)
	{
		return SpawnInternal(prefab, null, position, rotation);
	}

	public static GameObject Spawn(this GameObject prefab, Transform parent, Vector3 position)
	{
		return SpawnInternal(prefab, parent, position, Quaternion.identity);
	}

	public static GameObject Spawn(this GameObject prefab, Vector3 position)
	{
		return SpawnInternal(prefab, null, position, Quaternion.identity);
	}

	public static GameObject Spawn(this GameObject prefab, Transform parent)
	{
		return SpawnInternal(prefab, parent, Vector3.zero, Quaternion.identity);
	}

	public static GameObject Spawn(this GameObject prefab)
	{
		return SpawnInternal(prefab, null, Vector3.zero, Quaternion.identity);
	}

	public static void Recycle<T>(this T obj) where T : Component
	{
		GameObject.Destroy(obj);
	}

	public static void Recycle(this GameObject obj)
	{
		GameObject.Destroy(obj);
	}

	public static void RecycleAll<T>(this T prefab) where T : Component
	{
		GameObject.Destroy(prefab);
	}

	public static void RecycleAll(this GameObject prefab)
	{
		GameObject.Destroy(prefab);
	}
}
