namespace WeaverCore
{
    public static class ListCache<T>
	{
		static T[] cachedArray;

		public static T[] GetCachedList(int minimumSize)
		{
			if (cachedArray == null || cachedArray.Length < minimumSize)
			{
				cachedArray = new T[minimumSize];
			}

			return cachedArray;
		}
	}
}
