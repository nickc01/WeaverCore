using System.IO;

namespace WeaverBuildTools
{
	public struct ResourceMetaData
	{
		public bool compressed;
		public string hash;

		public ResourceMetaData(bool Compressed, string Hash)
		{
			compressed = Compressed;
			hash = Hash;
		}

		public MemoryStream ToStream()
		{
			var stream = new MemoryStream();
			if (compressed)
			{
				stream.WriteByte(1);
			}
			else
			{
				stream.WriteByte(0);
			}
			foreach (var character in hash)
			{
				stream.WriteByte((byte)character);
			}
			stream.Position = 0;
			return stream;
		}

		public static ResourceMetaData FromStream(Stream stream)
		{
			ResourceMetaData meta;
			long oldPosition = stream.Position;
			stream.Position = 0;
			int compressed = stream.ReadByte();
			meta.compressed = compressed == 1;
			meta.hash = "";
			for (int i = 0; i < stream.Length - 1; i++)
			{
				meta.hash += (char)stream.ReadByte();
			}
			stream.Position = oldPosition;
			return meta;
		}
	}
}
