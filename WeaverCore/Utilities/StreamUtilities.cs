using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace WeaverCore.Utilities
{
	/// <summary>
	/// Contains some utility functions related to streams
	/// </summary>
	public static class StreamUtilities
	{
		/// <summary>
		/// Copies all data from one stream to another
		/// </summary>
		/// <param name="source">The source stream</param>
		/// <param name="destination">The destination stream</param>
		/// <param name="bufferSize">The buffer size. The data will be copied in chunks of this size</param>
		/// <param name="resetPosition">Should positions of the streams be reset when done?</param>
		public static void CopyTo(this Stream source, Stream destination, int bufferSize = 2048, bool resetPosition = true)
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

		/// <summary>
		/// Decompresses a compressed stream of data
		/// </summary>
		/// <param name="source">The source stream to extract from</param>
		/// <param name="destination">The stream to send the decompressed data to</param>
		public static void Decompress(this Stream source, Stream destination)
		{
			var decompressionStream = new GZipStream(source, CompressionMode.Decompress);
            CopyTo(decompressionStream, destination);
		}
	}
}
