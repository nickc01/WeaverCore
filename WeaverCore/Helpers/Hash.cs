using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WeaverCore.Helpers
{
	public static class Hash
	{
		public static string GetHash(Stream stream)
		{
			var oldPosition = stream.Position;
			stream.Position = 0;
			try
			{
				using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
				{
					return Convert.ToBase64String(sha1.ComputeHash(stream));
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
				return Convert.ToBase64String(sha1.ComputeHash(bytes));
			}
		}
	}
}
