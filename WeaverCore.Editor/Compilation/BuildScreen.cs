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
	/// <summary>
	/// A window that is used to let the user customize how a mod is getting built
	/// </summary>
	public class BuildScreen : EditorWindow
	{
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

		/// <summary>
		/// Contains all the configured build settings the user specified
		/// </summary>
		[Serializable]
		public class Settings
		{
			/// <summary>
			/// Is the WeaverCore assembly only getting built?
			/// </summary>
			public bool WeaverCoreOnly = false;

			/// <summary>
			/// The name of the mod to build
			/// </summary>
			public string ModName = PlayerSettings.productName;

			/// <summary>
			/// Is the mod being built with Windows support?
			/// </summary>
			public bool WindowsSupport = true;

			/// <summary>
			/// Is the mod being built with Mac support?
			/// </summary>
			public bool MacSupport = true;

			/// <summary>
			/// Is the mod being built with Linux support?
			/// </summary>
			public bool LinuxSupport = true;

			/// <summary>
			/// Should the game be started up when the mod finishes building?
			/// </summary>
			public bool StartGame = true;

			/// <summary>
			/// The folder the mod is being placed in when built
			/// </summary>
			public string BuildLocation = GameBuildSettings.Settings.ModsLocation;

			/// <summary>
			/// What kind of asset bundle compression is being applied to the mod?
			/// </summary>
			public AssetBundleCompressionType CompressionType = AssetBundleCompressionType.LZMA;

			/// <summary>
			/// Gets a list of all the build modes the user has specified
			/// </summary>
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

		/// <summary>
		/// Shows the build screen to the user
		/// </summary>
		/// <param name="weaverCoreOnly">Is weavercore only getting built?</param>
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
			BuildSettings.WeaverCoreOnly = weaverCoreOnly;
		}


		private void OnGUI()
		{
			if (!BuildSettings.WeaverCoreOnly)
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

		/// <summary>
		/// Begins the build process
		/// </summary>
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
			if (BuildSettings.WeaverCoreOnly)
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
