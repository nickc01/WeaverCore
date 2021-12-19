using System.Collections.Generic;
using UnityEditor;
using WeaverCore.Features;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
	public class E_Language_I : Language_I
    {


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


        public override string GetString(string sheetName, string convoName, string fallback = null)
        {
            foreach (var reg in GetAllRegistries())
            {
                foreach (var langTable in reg.GetFeatures<LanguageTable>())
                {
                    var result = langTable.GetString(sheetName, convoName, null);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return fallback;
        }

        public override string GetString(string convoName, string fallback = null)
        {
            return GetString("General", convoName, fallback);
        }

        public override bool HasString(string sheetName, string convoName)
        {
            foreach (var reg in GetAllRegistries())
            {
                foreach (var langTable in reg.GetFeatures<LanguageTable>())
                {
                    var result = langTable.GetString(sheetName, convoName, null);
                    if (result != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool HasString(string convoName)
        {
            return HasString("General", convoName);
        }

		public override string GetStringInternal(string convoName)
		{
            return string.Empty;
		}

		public override string GetStringInternal(string convoName, string sheetName)
		{
            return string.Empty;
        }
	}
}