using System;
using Language;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
	// Token: 0x0200000E RID: 14
	public class G_Language_I : Language_I
	{
		// Token: 0x06000033 RID: 51 RVA: 0x00002FC8 File Offset: 0x000011C8
		public override string GetString(string sheetName, string convoName, string fallback = null)
		{
			bool flag = this.HasString(sheetName, convoName);
			string result;
			if (flag)
			{
				result = global::Language.Language.Get(convoName, sheetName);
			}
			else
			{
				result = fallback;
			}
			return result;
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00002FF4 File Offset: 0x000011F4
		public override string GetString(string convoName, string fallback = null)
		{
			bool flag = this.HasString(convoName);
			string result;
			if (flag)
			{
				result = global::Language.Language.Get(convoName);
			}
			else
			{
				result = fallback;
			}
			return result;
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00003020 File Offset: 0x00001220
		public override bool HasString(string sheetName, string convoName)
		{
			return global::Language.Language.Has(convoName, sheetName);
		}

		// Token: 0x06000036 RID: 54 RVA: 0x0000303C File Offset: 0x0000123C
		public override bool HasString(string convoName)
		{
			return global::Language.Language.Has(convoName);
		}
	}
}
