using System;
using GlobalEnums;
using Language;
using Modding;
using WeaverCore.Attributes;
using WeaverCore.Features;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
	public class G_WeaverLanguage_I : WeaverLanguage_I
	{
		[OnRuntimeInit]
		static void Init()
        {
            ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
        }

        private static string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
        {
			var currentLanguage = (SupportedLanguages)Language.Language.CurrentLanguage();
            foreach (var lTable in Registry.GetAllFeatures<LanguageTable>(t => t.Language == currentLanguage && t.SheetTitle == sheetTitle))
            {
                if (lTable.HasString(key))
                {
					return lTable.GetString(key, orig);
                }
            }
			return orig;
        }

        public override SupportedLanguages GetCurrentLanguage()
        {
			return (SupportedLanguages)Language.Language.CurrentLanguage();
        }

        public override string GetString(string sheetTitle, string key, string fallback = null)
		{
            var result = Language.Language.Get(key, sheetTitle);

            if (string.IsNullOrEmpty(result))
            {
                result = fallback;
            }

            return result;
            /*bool flag = this.HasString(sheetTitle, key);
			string result;
			if (flag)
			{
				result = Language.Language.Get(key, sheetTitle);
			}
			else
			{
				result = fallback;
			}
			return result;*/
        }

		public override string GetString(string key, string fallback = null)
		{
            var result = Language.Language.Get(key);

			if (string.IsNullOrEmpty(result))
			{
				result = fallback;
			}

			return result;

            /*bool flag = this.HasString(key);
			string result;
			if (flag)
			{
				result = Language.Language.Get(key);
			}
			else
			{
				result = fallback;
			}
			return result;*/
        }

		public override string GetStringInternal(string key)
		{
			return Language.Language.GetInternal(key, "General");
		}

		public override string GetStringInternal(string key, string sheetTitle)
		{
			return Language.Language.GetInternal(key, sheetTitle);
		}

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
			return Language.Language.Has(key, sheetTitle);
		}

		public override bool HasString(string key)
		{
			var currentLanguage = GetCurrentLanguage();

			var allLangTables = Registry.GetAllFeatures<LanguageTable>();

			foreach (var lTable in Registry.GetAllFeatures<LanguageTable>(t => t.Language == currentLanguage && t.SheetTitle == "General"))
			{
				if (lTable.HasString(key))
				{
					return true;
				}
			}
			return Language.Language.Has(key);
		}
	}
}
