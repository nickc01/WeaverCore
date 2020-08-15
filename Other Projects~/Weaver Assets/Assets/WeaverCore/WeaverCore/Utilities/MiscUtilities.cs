using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Utilities
{
	public static class MiscUtilities
	{
		public static int CombineHashCodes(int h1, int h2)
		{
			return (((h1 << 5) + h1) ^ h2);
		}


		public static int CombineHashCodes<T1,T2>(T1 val1, T2 val2)
		{
			int h1 = val1.GetHashCode();
			int h2 = val2.GetHashCode();
			return (((h1 << 5) + h1) ^ h2);
		}

		public static void AdditiveHash(ref int hash, int otherHash)
		{
			if (hash == 0)
			{
				hash = 17;
			}
			hash = hash * 23 + otherHash;
		}

		public static void AdditiveHash<T>(ref int hash, T value)
		{
			AdditiveHash(ref hash, value.GetHashCode());
		}

	}
}
