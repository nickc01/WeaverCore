using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// A burst effect that is normally played when giving an item to an NPC
    /// </summary>
    public class GiveItemBurst : MonoBehaviour
    {
        static CachedPrefab<GiveItemBurst> _defaultPrefab = new CachedPrefab<GiveItemBurst>();

        public static GiveItemBurst DefaultPrefab
        {
            get
            {
                if (_defaultPrefab.Value == null)
                {
                    _defaultPrefab.Value = WeaverAssets.LoadWeaverAsset<GameObject>("Give Item Burst").GetComponent<GiveItemBurst>();
                }
                return _defaultPrefab.Value;
            }
        }

        /// <summary>
        /// Spawns a "Give Item Burst" effect at the specified position
        /// </summary>
        /// <param name="position">The position to spawn the effect at</param>
        /// <param name="prefab">The prefab to use. If set to null, will use the <see cref="DefaultPrefab"/></param>
        /// <returns></returns>
        public static GiveItemBurst Spawn(Vector3 position, GiveItemBurst prefab = null)
        {
            if (prefab == null)
            {
                prefab = DefaultPrefab;
            }
            var instance = Pooling.Instantiate(prefab, position, Quaternion.identity);
            GameObjectUtilities.ActivateAllChildren(instance.gameObject, true);
            return instance;
        }
    }
}
