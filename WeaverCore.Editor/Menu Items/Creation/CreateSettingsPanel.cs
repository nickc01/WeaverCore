using UnityEditor;
using WeaverCore.Editor.Settings;
using WeaverCore.Settings;

namespace WeaverCore.Editor
{
    public static class CreateSettingsPanel
	{
		[MenuItem("WeaverCore/Create/Global Settings")]
		public static void CreateGlobalSettingsMenuItem()
		{
			CreateScriptableObjectWindow.OpenCreationMenu("Create Global Settings", typeof(GlobalSettings), "No Global Setting Types have been found.\nYou can create one by creating a new script in the Assets folder,\nand having the type inherit from WeaverCore.Settings.GlobalSettings", "Global Settings");
		}

		[MenuItem("WeaverCore/Create/Save Specific Settings")]
		public static void CreateSaveSpecificSettingsMenuItem()
		{
			CreateScriptableObjectWindow.OpenCreationMenu("Create Save Settings", typeof(SaveSpecificSettings), "No Save Specific Settings Types have been found.\nYou can create one by creating a new script in the Assets folder,\nand having the type inherit from WeaverCore.Settings.SaveSpecificSettings", "Save Specific Settings");
		}
	}
}
