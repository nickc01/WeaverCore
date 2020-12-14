using UnityEditor;

namespace TMPro.EditorUtilities
{
	public class TMPro_PackageImportPostProcessor : AssetPostprocessor
	{
		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			for (int i = 0; i < deletedAssets.Length; i++)
			{
				if (deletedAssets[i] == "Assets/TextMesh Pro")
				{
					string scriptingDefineSymbolsForGroup = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
					if (scriptingDefineSymbolsForGroup.Contains("TMP_PRESENT;"))
					{
						scriptingDefineSymbolsForGroup = scriptingDefineSymbolsForGroup.Replace("TMP_PRESENT;", "");
						PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, scriptingDefineSymbolsForGroup);
					}
					else if (scriptingDefineSymbolsForGroup.Contains("TMP_PRESENT"))
					{
						scriptingDefineSymbolsForGroup = scriptingDefineSymbolsForGroup.Replace("TMP_PRESENT", "");
						PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, scriptingDefineSymbolsForGroup);
					}
				}
			}
		}
	}
}
