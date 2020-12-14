using System.Runtime.InteropServices;

namespace TMPro.EditorUtilities
{
	public struct FT_FaceInfo
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
		public string name;

		public int pointSize;

		public int padding;

		public float lineHeight;

		public float baseline;

		public float ascender;

		public float descender;

		public float centerLine;

		public float underline;

		public float underlineThickness;

		public int characterCount;

		public int atlasWidth;

		public int atlasHeight;
	}
}
