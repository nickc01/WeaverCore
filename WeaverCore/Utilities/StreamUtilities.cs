using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WeaverCore.Utilities
{
	public static class StreamUtilities
	{
		public static string GetHash(this Stream stream)
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

		public static string GetHash(string filePath)
		{
			using (var fileStream = File.OpenRead(filePath))
			{
				return GetHash(fileStream);
			}
		}
	}
}
