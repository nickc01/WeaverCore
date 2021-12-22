using AssetsTools.NET;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Settings;
using WeaverCore.Editor.Utilities;

namespace WeaverCore.Editor.Compilation
{
	public class BuildScreen : EditorWindow
	{
		public bool WeaverCoreOnly { get; private set; }

		public const int WindowWidth = 400;
		public const int WindowHeight = 400;

		static Settings _settings;
		public static Settings BuildSettings
		{
			get
			{
				if (_settings == null)
				{
					LoadSettings();
				}
				return _settings;
			}
			set => _settings = value;
		}

		static void LoadSettings()
		{
			if (PersistentData.TryGetData(out Settings result))
			{
				_settings = result;
			}
			else
			{
				_settings = new Settings();
			}
		}

		[Serializable]
		public class Settings
		{
			public string ModName = PlayerSettings.productName;
			public bool WindowsSupport = true;
			public bool MacSupport = true;
			public bool LinuxSupport = true;
			public bool StartGame = true;
			public string BuildLocation = GameBuildSettings.Settings.ModsLocation;
			public AssetBundleCompressionType CompressionType = AssetBundleCompressionType.LZ4;

			public IEnumerable<BuildTarget> GetBuildModes()
			{
				if (WindowsSupport)
				{
					yield return BuildTarget.StandaloneWindows;
				}
				if (MacSupport)
				{
					yield return BuildTarget.StandaloneOSX;
				}
				if (LinuxSupport)
				{
					yield return BuildTarget.StandaloneLinux64;
				}
			}
		}


		public static void ShowBuildScreen(bool weaverCoreOnly)
		{
			var window = GetWindow<BuildScreen>();
			window.titleContent = new GUIContent("Build Settings");
			LoadSettings();
			window.Show();

			var resolution = Screen.currentResolution;

			var x = (resolution.width / 2) - (WindowWidth / 2);
			var y = (resolution.height / 2) - (WindowHeight / 2);

			window.position = new Rect(x, y, WindowWidth, WindowHeight);
			window.WeaverCoreOnly = weaverCoreOnly;
		}


		private void OnGUI()
		{
			if (!WeaverCoreOnly)
			{
				EditorGUILayout.LabelField("Mod Name");
				BuildSettings.ModName = EditorGUILayout.TextField(BuildSettings.ModName);
				EditorGUILayout.Space();
			}

			EditorGUILayout.LabelField("Windows Support");
			BuildSettings.WindowsSupport = EditorGUILayout.Toggle(BuildSettings.WindowsSupport);

			EditorGUILayout.LabelField("Mac Support");
			BuildSettings.MacSupport = EditorGUILayout.Toggle(BuildSettings.MacSupport);

			EditorGUILayout.LabelField("Linux Support");
			BuildSettings.LinuxSupport = EditorGUILayout.Toggle(BuildSettings.LinuxSupport);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Compression Method");
			BuildSettings.CompressionType = (AssetBundleCompressionType)EditorGUILayout.EnumPopup(BuildSettings.CompressionType);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Auto-Start Game");
			BuildSettings.StartGame = EditorGUILayout.Toggle(BuildSettings.StartGame);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Build Location");
			EditorGUILayout.BeginHorizontal();
			BuildSettings.BuildLocation = EditorGUILayout.TextField(BuildSettings.BuildLocation);
			if (GUILayout.Button("...", GUILayout.MaxWidth(30)))
			{
				string buildLocation = SelectionUtilities.SelectFolder("Select where the mod should be built", Environment.CurrentDirectory);
				if (buildLocation != null && buildLocation != "")
				{
					var directoryInfo = new DirectoryInfo(buildLocation);

					BuildSettings.BuildLocation = directoryInfo.FullName;
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Close"))
			{
				Close();
			}
			if (GUILayout.Button("Compile"))
			{
				Build();
			}

			EditorGUILayout.EndHorizontal();
		}

		void Build()
		{
			Close();
			BuildSettings.ModName = BuildSettings.ModName.Replace(" ", "");
			PersistentData.StoreData(BuildSettings);
			foreach (var target in BuildSettings.GetBuildModes())
			{
				if (!PlatformUtilities.IsPlatformSupportLoaded(target))
				{
					Debug.LogError($"The module for building for the following platform {target} is not installed. This platform will be skipped");
				}
			}
			if (WeaverCoreOnly)
			{
				BuildTools.BuildWeaverCore();
			}
			else
			{
				BuildTools.BuildMod();
			}
		}
	}
}
