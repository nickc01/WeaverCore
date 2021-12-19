using UnityEditor;
using WeaverCore.Editor.Utilities;
using WeaverCore.Features;

namespace WeaverCore.Editor
{
    public static class CreateLanguageTable
    {
		[MenuItem("WeaverCore/Create/Language Table")]
		public static void CreateLanguageTableItem()
		{
			AssetUtilities.CreateScriptableObject<LanguageTable>();
		}
	}
}
