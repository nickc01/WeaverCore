using UnityEditor;
using WeaverCore.Settings;

namespace WeaverCore.Editor
{
	public static class CreateSettingsPanel
	{
		[MenuItem("WeaverCore/Create/Settings Panel")]
		public static void CreateModSettingsMenuItem()
		{
			CreateScriptableObjectWindow.OpenCreationMenu("Create Panel", typeof(Panel), "No Panel Types have been found.\nYou can create one by creating a new script in the Assets folder,\nand having the type inherit from WeaverCore.Settings.Panel", "Panel");
		}

		[MenuItem("WeaverCore/Create/Save Specific Settings")]
		public static void CreateSaveSpecificSettingsMenuItem()
		{
			CreateScriptableObjectWindow.OpenCreationMenu("Create Save Settings", typeof(SaveSpecificSettings), "No Save Specific Settings Types have been found.\nYou can create one by creating a new script in the Assets folder,\nand having the type inherit from WeaverCore.Settings.SaveSpecificSettings", "Save Specific Settings");
		}
	}
}
