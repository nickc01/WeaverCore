using System;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore
{
	// Token: 0x02000028 RID: 40
	public static class Language
	{
		// Token: 0x060000BC RID: 188 RVA: 0x00004D14 File Offset: 0x00002F14
		public static string GetString(string sheetName, string convoName, string fallback = null)
		{
			return Language.impl.GetString(sheetName, convoName, fallback);
		}

		// Token: 0x060000BD RID: 189 RVA: 0x00004D23 File Offset: 0x00002F23
		public static string GetString(string convoName, string fallback = null)
		{
			return Language.impl.GetString(convoName, fallback);
		}

		// Token: 0x060000BE RID: 190 RVA: 0x00004D31 File Offset: 0x00002F31
		public static bool HasString(string sheetName, string convoName)
		{
			return Language.impl.HasString(sheetName, convoName);
		}

		// Token: 0x060000BF RID: 191 RVA: 0x00004D3F File Offset: 0x00002F3F
		public static bool HasString(string convoName)
		{
			return Language.impl.HasString(convoName);
		}

		// Token: 0x040000A1 RID: 161
		private static Language_I impl = ImplFinder.GetImplementation<Language_I>();
	}
}
