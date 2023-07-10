using UnityEditor;
using WeaverCore.Editor.Utilities;
using WeaverCore.Features;
using WeaverCore.Settings;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{
	public static class CreateMenu
	{
		[MenuItem("WeaverCore/Create/Language Table")]
		public static void CreateLanguageTableItem()
		{
			AssetUtilities.CreateScriptableObject<LanguageTable>();
		}

		[MenuItem("WeaverCore/Create/Registry")]
		static void CreateRegistryMenuItem()
		{
			AssetUtilities.CreateScriptableObject<Registry>();
		}

		[MenuItem("WeaverCore/Create/Atmos Pack")]
		static void CreateAtmosPackMenuItem()
		{
			AssetUtilities.CreateScriptableObject<AtmosPack>();
		}

		[MenuItem("WeaverCore/Create/Music Pack")]
		static void CreateMusicPackMenuItem()
		{
			AssetUtilities.CreateScriptableObject<MusicPack>();
		}

		[MenuItem("WeaverCore/Create/Scene Record")]
		static void CreateSceneRecordMenuItem()
		{
			AssetUtilities.CreateScriptableObject<SceneRecord>();
		}

		[MenuItem("WeaverCore/Create/Weaver Animation Data")]
		static void CreateWeaverAnimationDataMenuItem()
		{
			AssetUtilities.CreateScriptableObject<WeaverAnimationData>();
		}

        /*[MenuItem("WeaverCore/Create/Custom Charm")]
        static void CreateCustomCharmMenuItem()
        {
            AssetUtilities.CreateScriptableObject<CustomCharm>();
        }*/

        [MenuItem("WeaverCore/Create/Global Settings")]
		public static void CreateModSettingsMenuItem()
		{
			CreateScriptableObjectWindow.OpenCreationMenu("Create Global Settings", typeof(GlobalSettings), $"No Global Setting Types have been found.\nYou can create one by creating a new script in the Assets folder,\nand having the type inherit from {nameof(WeaverCore.Settings.GlobalSettings)}", "Global Settings");
		}

		[MenuItem("WeaverCore/Create/Save Specific Settings")]
		public static void CreateSaveSpecificSettingsMenuItem()
		{
			CreateScriptableObjectWindow.OpenCreationMenu("Create Save Settings", typeof(SaveSpecificSettings), $"No Save Specific Settings Types have been found.\nYou can create one by creating a new script in the Assets folder,\nand having the type inherit from {nameof(WeaverCore.Settings.SaveSpecificSettings)}", "Save Specific Settings");
		}

        [MenuItem("WeaverCore/Create/Weaver Charm")]
        public static void CreateWeaverCharmMenuItem()
        {
            CreateScriptableObjectWindow.OpenCreationMenu("Create Weaver Charm", typeof(WeaverCharm), $"No Weaver Charm Types have been found.\nYou can create one by creating a new script in the Assets folder,\nand having the type inherit from {nameof(WeaverCore.Features.WeaverCharm)}", "Weaver Charm");
        }
    }
}
