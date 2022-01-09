using GlobalEnums;
using Language;
using System.Collections.Generic;
using UnityEditor;
using WeaverCore.Features;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
	public class E_WeaverLanguage_I : WeaverLanguage_I
    {

        public override SupportedLanguages GetCurrentLanguage()
        {
            return SupportedLanguages.EN;
        }

        static IEnumerable<Registry> GetAllRegistries()
        {
            var guids = AssetDatabase.FindAssets("t:Registry");
            foreach (var id in guids)
            {
                var registry = AssetDatabase.LoadAssetAtPath<Registry>(AssetDatabase.GUIDToAssetPath(id));
                if (registry != null)
                {
                    yield return registry;
                }
            }
        }


        public override string GetString(string sheetTitle, string key, string fallback = null)
        {
            foreach (var reg in GetAllRegistries())
            {
                foreach (var langTable in reg.GetFeatures<LanguageTable>())
                {
                    if (langTable.Language == GetCurrentLanguage() && langTable.SheetTitle == sheetTitle)
                    {
                        var result = langTable.GetString(key, null);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }
            return fallback;
        }

        public override string GetString(string key, string fallback = null)
        {
            return GetString("General", key, fallback);
        }

        public override bool HasString(string sheetTitle, string key)
        {
            foreach (var reg in GetAllRegistries())
            {
                foreach (var langTable in reg.GetFeatures<LanguageTable>())
                {
                    if (langTable.Language == GetCurrentLanguage() && langTable.SheetTitle == sheetTitle)
                    {
                        var result = langTable.GetString(key, null);
                        if (result != null)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override bool HasString(string key)
        {
            return HasString("General", key);
        }

		public override string GetStringInternal(string key)
		{
            return string.Empty;
		}

		public override string GetStringInternal(string key, string sheetTitle)
		{
            return string.Empty;
        }
	}
}