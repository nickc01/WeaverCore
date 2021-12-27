using System;
using WeaverCore.Implementations;

namespace WeaverCore
{
    // Token: 0x02000028 RID: 40
    public static class WeaverLanguage
	{
		// Token: 0x060000BC RID: 188 RVA: 0x00004D14 File Offset: 0x00002F14
		public static string GetString(string key, string sheetTitle, string fallback = null)
		{
			return WeaverLanguage.impl.GetString(sheetTitle, key, fallback);
		}

		// Token: 0x060000BD RID: 189 RVA: 0x00004D23 File Offset: 0x00002F23
		public static string GetString(string key, string fallback = null)
		{
			return WeaverLanguage.impl.GetString(key, fallback);
		}

		// Token: 0x060000BE RID: 190 RVA: 0x00004D31 File Offset: 0x00002F31
		public static bool HasString(string key, string sheetTitle)
		{
			return WeaverLanguage.impl.HasString(sheetTitle, key);
		}

		// Token: 0x060000BF RID: 191 RVA: 0x00004D3F File Offset: 0x00002F3F
		public static bool HasString(string key)
		{
			return WeaverLanguage.impl.HasString(key);
		}

		public static string GetStringInternal(string key, string sheetTitle)
		{
			return impl.GetStringInternal(key, sheetTitle);
		}

		public static string GetStringInternal(string key)
		{
			return impl.GetStringInternal(key);
		}

		// Token: 0x040000A1 RID: 161
		private static WeaverLanguage_I impl = ImplFinder.GetImplementation<WeaverLanguage_I>();
	}
}
