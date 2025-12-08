using UnityEngine;

namespace WeaverCore
{
    public static class HitCache
	{
		static RaycastHit2D[] singleCache;

        public static RaycastHit2D[] GetSingleCachedArray()
		{
			if (singleCache == null)
			{
				singleCache = new RaycastHit2D[1];
            }

			return singleCache;
		}

		public static RaycastHit2D[] GetMultiCachedArray(int minimumSize)
		{
			return ListCache<RaycastHit2D>.GetCachedList(minimumSize);
        }

		public static Collider2D[] GetMultiCachedColliderArray(int minimumSize)
		{
			return ListCache<Collider2D>.GetCachedList(minimumSize);
        }
    }
}
