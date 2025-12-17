namespace WeaverCore
{
    /// <summary>
    /// Simple reusable array cache. Supports both shared (static) access for backwards compatibility and instance-based caches.
    /// </summary>
    public class ListCache<T>
    {
        static T[] sharedCachedArray;

        T[] instanceCachedArray;

        /// <summary>
        /// Gets a cached array for this instance.
        /// </summary>
        /// <param name="minimumSize">Minimum required length.</param>
        public T[] Get(int minimumSize)
        {
            if (instanceCachedArray == null || instanceCachedArray.Length < minimumSize)
            {
                instanceCachedArray = new T[minimumSize];
            }

            return instanceCachedArray;
        }

        /// <summary>
        /// Gets a cached array from the shared cache. Preserves existing API.
        /// </summary>
        /// <param name="minimumSize">Minimum required length.</param>
        public static T[] GetCachedList(int minimumSize)
        {
            if (sharedCachedArray == null || sharedCachedArray.Length < minimumSize)
            {
                sharedCachedArray = new T[minimumSize];
            }

            return sharedCachedArray;
        }
    }
}
