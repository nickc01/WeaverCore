using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WeaverCore.Utilities
{
	public static class HashUtilities
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

		public static string GetHash(Stream stream)
		{
			var oldPosition = stream.Position;
			stream.Position = 0;
			try
			{
				using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
				{
					var result = Convert.ToBase64String(sha1.ComputeHash(stream));
					result = result.Replace("\\", "_");
					result = result.Replace("/", "'");

					return result;
				}
			}
			finally
			{
				stream.Position = oldPosition;
			}
		}

		public static string GetHash(byte[] bytes)
		{
			using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
			{
				var result = Convert.ToBase64String(sha1.ComputeHash(bytes));

				result = result.Replace("\\", "_");
				result = result.Replace("/", "'");

				return result;
			}
		}

		public static string GetHash(string filePath)
		{
			using (var stream = File.OpenRead(filePath))
			{
				return GetHash(stream);
			}
		}
	}
}
