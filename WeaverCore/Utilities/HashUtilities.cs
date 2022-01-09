using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WeaverCore.Utilities
{
	/// <summary>
	/// Contains many utility functions for calculating hashes
	/// </summary>
	public static class HashUtilities
	{
		/// <summary>
		/// Combines two hash codes together
		/// </summary>
		/// <param name="h1">The first hash code</param>
		/// <param name="h2">The second has code</param>
		/// <returns>Returns the combined hash code</returns>
		public static int CombineHashCodes(int h1, int h2)
		{
			if (h1 == 0)
			{
				h1 = 17;
			}
			return h1 * 23 + h2;
		}

		/// <summary>
		/// Combines the hash codes of the two types together
		/// </summary>
		/// <typeparam name="T1">The type of the first parameter</typeparam>
		/// <typeparam name="T2">The type of the second parameter</typeparam>
		/// <param name="val1">The first value to get the hash code from</param>
		/// <param name="val2">The second value to get the hash code from</param>
		/// <returns>Returns the combine hash code of the two values</returns>
		public static int CombineHashCodes<T1,T2>(T1 val1, T2 val2)
		{
			int h1 = val1.GetHashCode();
			int h2 = val2.GetHashCode();
			return CombineHashCodes(h1, h2);
		}

		/// <summary>
		/// Combines the <paramref name="otherHash"/> into the first <paramref name="hash"/>
		/// </summary>
		public static void AdditiveHash(ref int hash, int otherHash)
		{
			if (hash == 0)
			{
				hash = 17;
			}
			hash = hash * 23 + otherHash;
		}

		/// <summary>
		/// Combines the hash code of <paramref name="value"/> into the first <paramref name="hash"/>
		/// </summary>
		/// <typeparam name="T">The type of the value to get the second hash code from</typeparam>
		/// <param name="hash">The original hash to combine with</param>
		/// <param name="value">The value to get the second hash code from</param>
		public static void AdditiveHash<T>(ref int hash, T value)
		{
			AdditiveHash(ref hash, value.GetHashCode());
		}

		/// <summary>
		/// Gets the hash code for an entire stream of data
		/// </summary>
		/// <param name="stream">The stream to get the hash code from</param>
		/// <returns>Returns the hash code of the entire stream</returns>
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

		/// <summary>
		/// Gets the hash code for an array of byte data
		/// </summary>
		/// <param name="bytes">The byte data to get the hash code from</param>
		/// <returns>Returns the hash code for the array of byte data</returns>
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

		/// <summary>
		/// Gets the hash code for an entire fire
		/// </summary>
		/// <param name="filePath">The path of the file to open</param>
		/// <returns>Returns the hash code for the entire file</returns>
		public static string GetHash(string filePath)
		{
			using (var stream = File.OpenRead(filePath))
			{
				return GetHash(stream);
			}
		}
	}
}
