using System;
using GlobalEnums;
using Language;
using Modding;
using WeaverCore.Attributes;
using WeaverCore.Features;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
	// Token: 0x0200000E RID: 14
	public class G_WeaverLanguage_I : WeaverLanguage_I
	{
		[OnRuntimeInit]
		static void Init()
        {
            ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
        }

        private static string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
        {
			var currentLanguage = (SupportedLanguages)global::Language.Language.CurrentLanguage();
			WeaverLog.Log("Key = " + key);
			WeaverLog.Log("Sheet Title = " + sheetTitle);
			WeaverLog.Log("Orig = " + orig);
			WeaverLog.Log("Current Language = " + currentLanguage);
            foreach (var lTable in Registry.GetAllFeatures<LanguageTable>(t => t.Language == currentLanguage && t.SheetTitle == sheetTitle))
            {
				WeaverLog.Log("Language Table Found = " + lTable.name);
				WeaverLog.Log("Has Key = " + lTable.HasString(key));
                if (lTable.HasString(key))
                {
					return lTable.GetString(key, orig);
                }
            }
			return orig;
        }

        public override SupportedLanguages GetCurrentLanguage()
        {
			return (SupportedLanguages)global::Language.Language.CurrentLanguage();
        }

        // Token: 0x06000033 RID: 51 RVA: 0x00002FC8 File Offset: 0x000011C8
        public override string GetString(string sheetTitle, string key, string fallback = null)
		{
			bool flag = this.HasString(sheetTitle, key);
			string result;
			if (flag)
			{
				result = global::Language.Language.Get(key, sheetTitle);
			}
			else
			{
				result = fallback;
			}
			return result;
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00002FF4 File Offset: 0x000011F4
		public override string GetString(string key, string fallback = null)
		{
			bool flag = this.HasString(key);
			WeaverLog.Log($"HAS KEY {key} = {flag}");
			string result;
			if (flag)
			{
				result = global::Language.Language.Get(key);
			}
			else
			{
				result = fallback;
			}
			WeaverLog.Log("Result = " + result);
			return result;
		}

		public override string GetStringInternal(string key)
		{
			return global::Language.Language.GetInternal(key, "General");
		}

		public override string GetStringInternal(string key, string sheetTitle)
		{
			return global::Language.Language.GetInternal(key, sheetTitle);
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00003020 File Offset: 0x00001220
		public override bool HasString(string sheetTitle, string key)
		{
			var currentLanguage = GetCurrentLanguage();
			foreach (var lTable in Registry.GetAllFeatures<LanguageTable>(t => t.Language == currentLanguage && t.SheetTitle == sheetTitle))
            {
                if (lTable.HasString(key))
                {
					return true;
                }
            }
			return global::Language.Language.Has(key, sheetTitle);
		}

		// Token: 0x06000036 RID: 54 RVA: 0x0000303C File Offset: 0x0000123C
		public override bool HasString(string key)
		{
			var currentLanguage = GetCurrentLanguage();

			var allLangTables = Registry.GetAllFeatures<LanguageTable>();
            foreach (var lTable in allLangTables)
            {
				WeaverLog.Log("ALL LANG TABLE = " + lTable.name);
            }

			foreach (var lTable in Registry.GetAllFeatures<LanguageTable>(t => t.Language == currentLanguage && t.SheetTitle == "General"))
			{
				WeaverLog.Log("FOUND TABLE = " + lTable.name);
				if (lTable.HasString(key))
				{
					return true;
				}
			}
			return global::Language.Language.Has(key);
		}
	}
}
