using UnityEngine;

namespace WeaverCore
{
    /// <summary>
    /// Reusable caches for raycast/overlap queries. Provides both shared (static) and instance-based buffers.
    /// </summary>
    public class HitCache
	{
		static RaycastHit2D[] sharedSingleCache;
        static readonly HitCache Shared = new HitCache();

        RaycastHit2D[] singleCache;
        readonly ListCache<RaycastHit2D> raycastCache = new ListCache<RaycastHit2D>();
        readonly ListCache<Collider2D> colliderCache = new ListCache<Collider2D>();

        /// <summary>
        /// Retrieves a single-element cache for this instance.
        /// </summary>
        public RaycastHit2D[] GetSingle()
		{
			if (singleCache == null)
			{
				singleCache = new RaycastHit2D[1];
            }

			return singleCache;
		}

        /// <summary>
        /// Retrieves a multi-hit cache for this instance.
        /// </summary>
        public RaycastHit2D[] GetMulti(int minimumSize)
        {
            return raycastCache.Get(minimumSize);
        }

        /// <summary>
        /// Retrieves a collider cache for this instance.
        /// </summary>
        public Collider2D[] GetColliderMulti(int minimumSize)
        {
            return colliderCache.Get(minimumSize);
        }

        /// <summary>
        /// Shared single-element cache (backwards compatible).
        /// </summary>
        public static RaycastHit2D[] GetSingleCachedArray()
		{
			if (sharedSingleCache == null)
			{
				sharedSingleCache = new RaycastHit2D[1];
            }

			return sharedSingleCache;
		}

        /// <summary>
        /// Shared multi-hit cache (backwards compatible).
        /// </summary>
		public static RaycastHit2D[] GetMultiCachedArray(int minimumSize)
		{
			return Shared.GetMulti(minimumSize);
        }

        /// <summary>
        /// Shared collider cache (backwards compatible).
        /// </summary>
		public static Collider2D[] GetMultiCachedColliderArray(int minimumSize)
		{
			return Shared.GetColliderMulti(minimumSize);
        }
    }
}
