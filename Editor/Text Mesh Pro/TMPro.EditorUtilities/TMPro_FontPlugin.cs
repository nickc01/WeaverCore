using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	public class TMPro_FontPlugin
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate void DebugLog(string log);

		private static readonly DebugLog debugLog = DebugWrapper;

		private static readonly IntPtr functionPointer = Marshal.GetFunctionPointerForDelegate(debugLog);

		private static void DebugWrapper(string log)
		{
			Debug.Log(log);
		}

		public static void LinkDebugLog()
		{
			LinkDebug(functionPointer);
		}

		[DllImport("TMPro_Plugin")]
		private static extern void LinkDebug([MarshalAs(UnmanagedType.FunctionPtr)] IntPtr debugCall);

		[DllImport("TMPro_Plugin")]
		public static extern int Initialize_FontEngine();

		[DllImport("TMPro_Plugin")]
		public static extern int Destroy_FontEngine();

		[DllImport("TMPro_Plugin")]
		public static extern int Load_TrueType_Font(string fontPath);

		[DllImport("TMPro_Plugin")]
		public static extern int FT_Size_Font(int fontSize);

		[DllImport("TMPro_Plugin")]
		public static extern int Render_Character(byte[] buffer_fill, byte[] buffer_edge, int buffer_width, int buffer_height, int offset, int asc, FaceStyles style, float thickness, RenderModes rasterMode, ref FT_GlyphInfo glyphInfo);

		[DllImport("TMPro_Plugin")]
		public static extern int Render_Characters(byte[] buffer, int buffer_width, int buffer_height, int character_padding, int[] asc_set, int char_count, FaceStyles style, float style_mod, bool autoSize, RenderModes renderMode, int method, ref FT_FaceInfo fontData, FT_GlyphInfo[] Output);

		[DllImport("TMPro_Plugin")]
		public static extern int FT_GetKerningPairs(string fontPath, int[] characterSet, int setCount, FT_KerningPair[] kerningPairs);

		[DllImport("TMPro_Plugin")]
		public static extern float Check_RenderProgress();
	}
}
