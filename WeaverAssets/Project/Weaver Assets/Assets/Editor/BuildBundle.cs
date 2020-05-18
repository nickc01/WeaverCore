using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using WeaverCore;
using WeaverCore.Editor.Visual;
using WeaverCore.Helpers;

namespace WeaverAssets
{
	public static class BuildBundle
	{
		static readonly ReadOnlyCollection<BuildMode> buildModes = new List<BuildMode>()
		{
			new BuildMode()
			{
				Extension = ".bundle.win",
				Target = BuildTarget.StandaloneWindows
			},
			new BuildMode()
			{
				Extension = ".bundle.mac",
				Target = BuildTarget.StandaloneOSX
			},
			new BuildMode()
			{
				Extension = ".bundle.unix",
				Target = BuildTarget.StandaloneLinuxUniversal
			}
		}.AsReadOnly();

		[MenuItem("Weaver Assets/Build %F5")]
		public static void Build()
		{
			BeginCompile();
		}

		/*static string[] GetFiles(string filter)
		{
			var EditorDirectory = new DirectoryInfo("Assets\\Editor").FullName;
			var WeaverEditorDirectory = new DirectoryInfo("Assets\\WeaverCore").FullName;
			var AssetsFolder = new DirectoryInfo("Assets").FullName;
			var files = Directory.GetFiles(AssetsFolder, filter, SearchOption.AllDirectories).ToList();
			for (int i = files.Count - 1; i >= 0; i--)
			{
				if (files[i].Contains(WeaverEditorDirectory) || files[i].Contains(EditorDirectory))
				{
					files.RemoveAt(i);
				}
			}
			return files.ToArray();
		}

		static string[] GetScripts()
		{
			return GetFiles("*.cs");
		}

		static string[] GetReferences()
		{
			return GetFiles("*.dll");
		}

		static bool PathEndsWithDirectorySeparator(string path)
		{
			if (path.Length == 0)
				return false;

			char lastChar = path[path.Length - 1];
			return lastChar == Path.DirectorySeparatorChar
				|| lastChar == Path.AltDirectorySeparatorChar;
		}

		static char GetDirectorySeparatorUsedInPath(string path)
		{
			if (path.Contains(Path.AltDirectorySeparatorChar))
				return Path.AltDirectorySeparatorChar;

			return Path.DirectorySeparatorChar;
		}*/

		/*static string PathAddBackslash(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			path = path.TrimEnd();

			if (PathEndsWithDirectorySeparator(path))
				return path;

			return path + GetDirectorySeparatorUsedInPath(path);
		}*/

		public static void BeginCompile()
		{
			var folder = new DirectoryInfo("Assets\\..\\..\\..\\..\\WeaverCore\\Resources\\AssetBundles").FullName;
			var temp = Path.GetTempPath();

			for (int modeIndex = 0; modeIndex < buildModes.Count; modeIndex++)
			{
				var mode = buildModes[modeIndex];
				if (IsPlatformSupportLoaded(mode.Target))
				{
					var bundleBuilds = new DirectoryInfo(temp + @"BundleBuilds\");
					if (bundleBuilds.Exists)
					{
						foreach (var existingFile in bundleBuilds.GetFiles())
						{
							existingFile.Delete();
						}
					}
					else
					{
						bundleBuilds.Create();
					}

					BuildPipeline.BuildAssetBundles(bundleBuilds.FullName, BuildAssetBundleOptions.None, mode.Target);

					foreach (var bundleFile in bundleBuilds.GetFiles())
					{
						if (bundleFile.Extension == "" && !bundleFile.Name.Contains("BundleBuilds"))
						{
							File.Copy(bundleFile.FullName, "Assets\\..\\..\\..\\..\\WeaverCore\\Resources\\AssetBundles\\weavercore" + mode.Extension, true);
						}
					}
				}
			}
		}

		/*static void PostBuild(string builtAssemblyPath)
		{
			//Console.SetOut(new DebugConsole());
			List<Type> ValidMods = Mods.GetMods();
			//List<Type> ValidMods = Assembly.Load("Assembly-CSharp").GetTypes().Where(type => typeof(IWeaverMod).IsAssignableFrom(type) && !type.IsAbstract && !type.IsGenericTypeDefinition && !type.IsInterface && !typeof(WeaverCoreMod).IsAssignableFrom(type)).ToList();
			AdjustMonoScripts();
			LoadAssemblyManipulator();
			var temp = Path.GetTempPath();
			var registries = RegistryChecker.LoadAllRegistries();
			try
			{
				foreach (var registry in registries)
				{
					registry.ReplaceAssemblyName("Assembly-CSharp", BuildSettingsScreen.RetrievedBuildSettings.ModName);
					registry.ApplyChanges();
					Debugger.Log("Registry Mod Assembly Name New = " + registry.GetString("modAssemblyName"));
				}
				for (int modeIndex = 0; modeIndex < buildModes.Count; modeIndex++)
				{
					var mode = buildModes[modeIndex];
					if (IsPlatformSupportLoaded(mode.Target))
					{
						Progress = Mathf.Lerp(0.1f, 1.0f, modeIndex / (float)buildModes.Count);
						if (IsPlatformSupportLoaded(mode.Target))
						{
							var bundleBuilds = new DirectoryInfo(temp + @"BundleBuilds\");
							if (bundleBuilds.Exists)
							{
								foreach (var existingFile in bundleBuilds.GetFiles())
								{
									existingFile.Delete();
								}
							}
							else
							{
								bundleBuilds.Create();
							}
							BuildPipeline.BuildAssetBundles(bundleBuilds.FullName, BuildAssetBundleOptions.None, mode.Target);
							ShowProgress();
							foreach (var bundleFile in bundleBuilds.GetFiles())
							{
								if (bundleFile.Extension == "" && !bundleFile.Name.Contains("BundleBuilds"))
								{
									//PostProcessBundle(builtAssemblyPath, bundleFile.FullName, bundleFile.Name + mode.Extension,bundleFile.Name);
									Embed("addresource", builtAssemblyPath, bundleFile.FullName, bundleFile.Name + mode.Extension, false);
								}
							}
						}
						else
						{
							Debug.LogWarning($"{mode.Target} module is not loaded, so building for the target is not available");
						}
					}
				}
				ClearProgress();
				foreach (var modType in ValidMods)
				{
					var instance = Activator.CreateInstance(modType) as IWeaverMod;
					AddMod(builtAssemblyPath, modType.Namespace, modType.Name, instance.Name, instance.Unloadable, BuildSettingsScreen.RetrievedBuildSettings.HollowKnightDirectory, typeof(WeaverCore.Internal.WeaverCore).Assembly.Location);
				}
				Debug.Log("Build Complete");
			}
			finally
			{
				foreach (var registry in registries)
				{
					registry.ReplaceAssemblyName(BuildSettingsScreen.RetrievedBuildSettings.ModName, "Assembly-CSharp");
					registry.ApplyChanges();
					Debugger.Log("Registry Mod Assembly Name Old = " + registry.GetString("modAssemblyName"));
				}
			}

			var hkEXE = PathAddBackslash(BuildSettingsScreen.RetrievedBuildSettings.HollowKnightDirectory) + "hollow_knight.exe";

			if (File.Exists(hkEXE))
			{
				System.Diagnostics.Process.Start(hkEXE);
			}
		}*/

		//Tests if a build target is available
		public static bool IsPlatformSupportLoaded(BuildTarget buildTarget)
		{
			var UnityEditor = System.Reflection.Assembly.Load("UnityEditor");
			var ModuleManagerT = UnityEditor.GetType("UnityEditor.Modules.ModuleManager");

			var buildString = (string)ModuleManagerT.GetMethod("GetTargetStringFromBuildTarget", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { buildTarget });
			return (bool)ModuleManagerT.GetMethod("IsPlatformSupportLoaded", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { buildString });

		}
	}
}