using WeaverCore.Attributes;

namespace WeaverCore.Game.Patches
{
    static class ObjectPool_Patches
	{
		[OnInit]
		static void Init()
		{
            On.ObjectPool.Spawn_GameObject_Transform_Vector3_Quaternion += ObjectPool_Spawn_GameObject_Transform_Vector3_Quaternion;
            On.ObjectPool.Recycle_GameObject += ObjectPool_Recycle_GameObject;
		}

        private static void ObjectPool_Recycle_GameObject(On.ObjectPool.orig_Recycle_GameObject orig, UnityEngine.GameObject obj)
        {
            if (obj.TryGetComponent<PoolableObject>(out var poolableObject))
            {
				poolableObject.ReturnToPool();
            }
            else
            {
                orig(obj);
            }
        }

        private static UnityEngine.GameObject ObjectPool_Spawn_GameObject_Transform_Vector3_Quaternion(On.ObjectPool.orig_Spawn_GameObject_Transform_Vector3_Quaternion orig, UnityEngine.GameObject prefab, UnityEngine.Transform parent, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation)
        {
			if (prefab.TryGetComponent<PoolableObject>(out var poolableObject))
			{
				var instance = Pooling.Instantiate(prefab, parent);
				instance.transform.localPosition = position;
				instance.transform.localRotation = rotation;
				return instance;
			}
			else
			{
				return orig(prefab, parent, position, rotation);
			}
        }
    }
}
