using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

		public static string GetHash(this byte[] data)
		{
			//var oldPosition = stream.Position;
			//stream.Position = 0;
			//try
			//{
				using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
				{
					return Convert.ToBase64String(sha1.ComputeHash(data));
				}
			//}
			//finally
			//{
				//stream.Position = oldPosition;
			//}
		}

		public static string GetHash(string filePath)
		{
			using (var fileStream = File.OpenRead(filePath))
			{
				return GetHash(fileStream);
			}
		}

		public static void CopyToStream(this Stream source, Stream destination, int bufferSize = 2048, bool resetPosition = true)
		{
			long oldPosition1 = -1;
			if (source.CanSeek)
			{
				oldPosition1 = source.Position;
			}
			long oldPosition2 = -1;
			if (destination.CanSeek)
			{
				oldPosition2 = destination.Position;
			}
			byte[] buffer = new byte[bufferSize];
			int read = 0;
			do
			{
				read = source.Read(buffer, 0, buffer.GetLength(0));
				if (read > 0)
				{
					destination.Write(buffer, 0, read);
				}
			} while (read != 0);
			if (resetPosition)
			{
				if (oldPosition1 != -1)
				{
					source.Position = oldPosition1;
				}
				if (oldPosition2 != -1)
				{
					destination.Position = oldPosition2;
				}
			}
		}

		public static void Decompress(this Stream source, Stream destination)
		{
			var decompressionStream = new GZipStream(source, CompressionMode.Decompress);
			decompressionStream.CopyToStream(destination);
		}
	}
}
