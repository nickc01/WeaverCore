using GlobalEnums;
using Language;
using System;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	// Token: 0x0200006C RID: 108
	public abstract class WeaverLanguage_I : IImplementation
	{
		public abstract SupportedLanguages GetCurrentLanguage();

		// Token: 0x060001EC RID: 492
		public abstract string GetString(string sheetName, string convoName, string fallback = null);

		// Token: 0x060001ED RID: 493
		public abstract string GetString(string convoName, string fallback = null);

		// Token: 0x060001EE RID: 494
		public abstract bool HasString(string sheetName, string convoName);

		// Token: 0x060001EF RID: 495
		public abstract bool HasString(string convoName);

		public abstract string GetStringInternal(string convoName);

		public abstract string GetStringInternal(string convoName, string sheetName);
	}
}
