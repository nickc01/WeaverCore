using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Awaiters;
using WeaverCore.Editor.Helpers;
using WeaverCore.Editor.Internal;
using WeaverCore.Utilities;
using WeaverCore.Interfaces;
using WeaverCore.Internal;
using UnityEditor.PackageManager;
using WeaverBuildTools.Commands;
using WeaverCore.Editor.Internal;

namespace WeaverCore.Editor
{
	/*class ConsolePatcher : IPatch
	{
		public void Patch(HarmonyPatcher patcher)
		{
			//Debugger.Log("Patch Test");
			patcher.Patch(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }),typeof(ConsolePatcher).GetMethod("Prefix",BindingFlags.NonPublic | BindingFlags.Static),null);
		}

		static bool Prefix(string value)
		{
			WeaverLog.Log(value);
			return true;
		}
	}*/


	public static class ModBuilder
	{
		//static Assembly AssemblyManipulator;

		//delegate void embedMethod(string mode, string sourceAssembly, string additionFile, string resourcePath, bool compress);
		//delegate void addModMethod(string assembly, string @namespace, string typeName, string modName, bool unloadable, string hollowKnightPath,string weaverCorePath );
		//delegate void replaceTypesMethod(string assemblyWithTypes, string sourceAssembly, List<string> assembliesToLook);

		//static embedMethod Embed;
		//static replaceTypesMethod ReplaceTypes;
		//static addModMethod AddMod;

		static float progress = 0.0f;

		static string AsmLocationRelative = "Assets\\WeaverCore\\WeaverCore.Editor";
		static DirectoryInfo AsmEditorLocation = new DirectoryInfo("Assets\\WeaverCore\\WeaverCore.Editor");

		//static bool lastBuildSuccessful = false;
		//static string lastBuildDestination;

		static float Progress
		{
			get
			{
				return progress;
			}
			set
			{
				progress = value;
				EditorUtility.DisplayProgressBar("Compiling", "Compiling Mod : " + BuildSettingsScreen.RetrievedBuildSettings.ModName, progress);
			}
		}

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

		static void ShowProgress()
		{
			Progress = progress;
		}

		static void ClearProgress()
		{
			EditorUtility.ClearProgressBar();
		}

		[MenuItem("WeaverCore/Compile %F5")]
		public static void Compile()
		{
			//Debugger.Log("AA");
			WeaverRoutine.Start(BeginCompile());
			/*BuildSettingsScreen.ChooseString((modName) =>
			{
				EditorRoutine.Start(BeginCompile(modName));
			});*/
		}

		/*public static void ClearLogConsole()
		{
			var assembly = System.Reflection.Assembly.GetAssembly(typeof(SceneView));
			Type logEntries = assembly.GetType("UnityEditor.LogEntries");
			MethodInfo clearConsoleMethod = logEntries.GetMethod("Clear");
			clearConsoleMethod.Invoke(new object(), null);
		}*/

		static DirectoryInfo SelectSaveLocation()
		{
			string buildFolder;
			if (File.Exists("LastUsedDirectory.dat"))
			{
				buildFolder = File.ReadAllText("LastUsedDirectory.dat");
			}
			else
			{
				if (BuildSettingsScreen.RetrievedBuildSettings.HollowKnightDirectory != null)
				{
					buildFolder = PathAddBackslash(BuildSettingsScreen.RetrievedBuildSettings.HollowKnightDirectory) + @"hollow_knight_Data\Managed\Mods\";
				}
				else
				{
					buildFolder = new DirectoryInfo("WeaverCore/bin").FullName;
				}
				//buildFolder = BuildSettingsScreen.RetrievedBuildSettings.HollowKnightDirectory;
				//buildFolder = new DirectoryInfo($"{nameof(WeaverCore)}/bin").FullName;
			}
			if (!Directory.Exists(buildFolder))
			{
				Directory.CreateDirectory(buildFolder);
			}
			var directory = EditorUtility.OpenFolderPanel("The folder of where you want the mod to be placed", buildFolder, "");
			if (directory == "")
			{
				return null;
			}
			using (var file = File.Open("LastUsedDirectory.dat", FileMode.Create))
			{
				using (var writer = new StreamWriter(file))
				{
					writer.Write(directory);
				}
			}
			return new DirectoryInfo(directory);
		}
		static List<string> GetModFiles(string filter)
		{
			var EditorDirectory = new DirectoryInfo("Assets\\WeaverCore").FullName;
			var AssetsFolder = new DirectoryInfo("Assets").FullName;
			var files = Directory.GetFiles(AssetsFolder, filter, SearchOption.AllDirectories).ToList();
			for (int i = files.Count - 1; i >= 0; i--)
			{
				var file = files[i];
				//Debug.Log("Mod Script = " + file);
				if (file.Contains("Assets\\WeaverCore") || file.Contains("Editor\\") || file.Contains("Assets/WeaverCore") || file.Contains("Editor/"))
				{
					//Debug.Log("Removed!");
					files.RemoveAt(i);
				}
			}
			return files;
		}

		static string[] GetModScripts(params string[] additions)
		{
			//return GetNoBuildFiles("*.cs").ToArray();
			var files = GetModFiles("*.cs");
			if (additions != null)
			{
				foreach (var addition in additions)
				{
					if (addition != "" && addition != null)
					{
						files.Add(addition);
					}
				}
			}
			return files.ToArray();
		}

		static string PathAddBackslash(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			path = path.TrimEnd();

			if (PathEndsWithDirectorySeparator(path))
				return path;

			return path + GetDirectorySeparatorUsedInPath(path);
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
		}

		public static IEnumerator<IWeaverAwaiter> BeginCompile()
		{
			yield return BuildSettingsScreen.RetrieveBuildSettings();

			if (BuildSettingsScreen.RetrievedBuildSettings == null)
			{
				yield break;
			}

			var folder = SelectSaveLocation();
			if (folder == null)
			{
				yield break;
			}

			var destination = PathAddBackslash(folder.FullName) + BuildSettingsScreen.RetrievedBuildSettings.ModName + ".dll";
			Progress = 0.0f;

			try
			{
				WeaverReloadTools.DoReloadTools = false;
				WeaverReloadTools.DoOnScriptReload = false;
				var modBuilder = new Builder();
				modBuilder.BuildPath = destination;
				modBuilder.ReferencePaths.Add(WeaverCoreBuilder.DefaultAssemblyCSharpLocation);
				modBuilder.ReferencePaths.Add(WeaverCoreBuilder.DefaultBuildLocation);
				modBuilder.ScriptPaths = GetModScripts().ToList();
				modBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/WeaverCore.dll");
				modBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/WeaverCore.Editor.dll");
				modBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/HollowKnight.dll");

				yield return modBuilder.Build();


				WeaverCoreBuilder.BuildFinishedVersion(folder.FullName + "\\WeaverCore.dll");


				File.Move(AsmEditorLocation.FullName + "\\WeaverCore.Editor.asmdef", AsmEditorLocation.FullName + "\\WeaverCore.Editor-ORIGINAL.txt");
				File.Move(AsmEditorLocation.FullName + "\\WeaverCore.Editor-COMPILEVERSION.txt", AsmEditorLocation.FullName + "\\WeaverCore.Editor.asmdef");

				AssetDatabase.ImportAsset(AsmLocationRelative + "\\WeaverCore.Editor.asmdef");

				yield return null;
				yield return null;
				//PostBuild(destination);
				//Console.SetOut(new DebugConsole());
				List <Type> ValidMods = Mods.GetMods();
				//List<Type> ValidMods = Assembly.Load("Assembly-CSharp").GetTypes().Where(type => typeof(WeaverMod).IsAssignableFrom(type) && !type.IsAbstract && !type.IsGenericTypeDefinition && !type.IsInterface && !typeof(WeaverCoreMod).IsAssignableFrom(type)).ToList();
				//WeaverLog.Log("Adjusting Scripts Before");
				AdjustMonoScripts(BuildSettingsScreen.RetrievedBuildSettings.ModName);
				AdjustMonoScripts("Assembly-CSharp", path => path.EndsWith(".cs") && !path.Contains("Editor/") && path.Contains("Hollow Knight/"));
				//LoadAssemblyManipulator();
				var temp = Path.GetTempPath();
				var registries = RegistryChecker.LoadAllRegistries();
				try
				{
					yield return null;
					foreach (var registry in registries)
					{
						//WeaverLog.Log("Modifying registry assembly before");
						registry.ReplaceAssemblyName("Assembly-CSharp", BuildSettingsScreen.RetrievedBuildSettings.ModName);
						registry.ApplyChanges();
						//Debugger.Log("Registry Mod Assembly Name New = " + registry.GetString("modAssemblyName"));
					}
					for (int modeIndex = 0; modeIndex < buildModes.Count; modeIndex++)
					{
						if (BuildSettingsScreen.RetrievedBuildSettings.SupportedBuildMode(buildModes[modeIndex]))
						{
							Progress = Mathf.Lerp(0.1f, 1.0f, modeIndex / (float)buildModes.Count);
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
								ShowProgress();
								foreach (var bundleFile in bundleBuilds.GetFiles())
								{
									if (bundleFile.Extension == "" && !bundleFile.Name.Contains("BundleBuilds"))
									{
										EmbedResourceCMD.EmbedResource(destination, bundleFile.FullName, bundleFile.Name + mode.Extension, compression: WeaverBuildTools.Enums.CompressionMethod.NoCompression);
									}
								}
							}
							else
							{
								Debug.LogWarning(mode.Target.ToString() + " module is not loaded, so building for the target is not available");
							}
						}
					}
					ClearProgress();
				}
				finally
				{
					foreach (var registry in registries)
					{
						AdjustMonoScripts("Assembly-CSharp");
						AdjustMonoScripts("HollowKnight", path => path.EndsWith(".cs") && !path.Contains("Editor/") && path.Contains("Hollow Knight/"));
						registry.ReplaceAssemblyName(BuildSettingsScreen.RetrievedBuildSettings.ModName, "Assembly-CSharp");
						registry.ApplyChanges();
					}

					File.Move(AsmEditorLocation.FullName + "\\WeaverCore.Editor.asmdef", AsmEditorLocation.FullName + "\\WeaverCore.Editor-COMPILEVERSION.txt");
					File.Move(AsmEditorLocation.FullName + "\\WeaverCore.Editor-ORIGINAL.txt", AsmEditorLocation.FullName + "\\WeaverCore.Editor.asmdef");

					AssetDatabase.ImportAsset(AsmLocationRelative + "\\WeaverCore.Editor.asmdef");
				}

				if (BuildSettingsScreen.RetrievedBuildSettings.StartGame)
				{
					//WeaverLog.Log("Launching Game");
					var hkEXE = new FileInfo(PathAddBackslash(BuildSettingsScreen.RetrievedBuildSettings.HollowKnightDirectory) + "hollow_knight.exe");


					if (hkEXE.FullName.Contains("steamapps"))
					{
						var steamDirectory = hkEXE.Directory;
						while (steamDirectory.Name != "Steam")
						{
							steamDirectory = steamDirectory.Parent;
							if (steamDirectory == null)
							{
								break;
							}
						}
						if (steamDirectory != null)
						{
							System.Diagnostics.Process.Start(steamDirectory.FullName + "\\steam.exe", "steam://rungameid/367520");
						}
						else
						{
							System.Diagnostics.Process.Start(hkEXE.FullName);
						}
					}
					else
					{
						System.Diagnostics.Process.Start(hkEXE.FullName);
					}
				}
			}
			finally
			{
				WeaverReloadTools.DoReloadTools = true;
				WeaverReloadTools.DoOnScriptReload = true;
			}
		}
	


		static void AdjustMonoScripts(string newAssemblyName, Func<string,bool> constraints = null)
		{
			if (constraints == null)
			{
				constraints = path => path.EndsWith(".cs") && !path.Contains("Editor/") && !path.Contains("Assets/WeaverCore/");
			}

			IEnumerable<string> assetFolderPaths = AssetDatabase.GetAllAssetPaths().Where(constraints);

			foreach (var path in assetFolderPaths)
			{
				MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

				if (script != null)
				{
					ChangeMonoScriptAssembly(script, newAssemblyName);
				}
			}
		}

		static void ChangeMonoScriptAssembly(MonoScript script, string assemblyName)
		{
			MethodInfo getNamespace = script.GetType().GetMethod("GetNamespace", BindingFlags.NonPublic | BindingFlags.Instance);
			MethodInfo dynMethod = script.GetType().GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance);
			dynMethod.Invoke(script, new object[] { script.text, script.name, (string)getNamespace.Invoke(script,null), assemblyName, false });
		}

		static void PostBuild(string builtAssemblyPath)
		{
			//Console.SetOut(new DebugConsole());
			List<Type> ValidMods = Mods.GetMods();
			//List<Type> ValidMods = Assembly.Load("Assembly-CSharp").GetTypes().Where(type => typeof(WeaverMod).IsAssignableFrom(type) && !type.IsAbstract && !type.IsGenericTypeDefinition && !type.IsInterface && !typeof(WeaverCoreMod).IsAssignableFrom(type)).ToList();
			//WeaverLog.Log("Adjusting Scripts Before");
			AdjustMonoScripts(BuildSettingsScreen.RetrievedBuildSettings.ModName);
			AdjustMonoScripts("Assembly-CSharp",path => path.EndsWith(".cs") && !path.Contains("Editor/") && path.Contains("Hollow Knight/"));
			//LoadAssemblyManipulator();
			var temp = Path.GetTempPath();
			var registries = RegistryChecker.LoadAllRegistries();
			try
			{
				foreach (var registry in registries)
				{
					//WeaverLog.Log("Modifying registry assembly before");
					registry.ReplaceAssemblyName("Assembly-CSharp", BuildSettingsScreen.RetrievedBuildSettings.ModName);
					registry.ApplyChanges();
					//Debugger.Log("Registry Mod Assembly Name New = " + registry.GetString("modAssemblyName"));
				}
				for (int modeIndex = 0; modeIndex < buildModes.Count; modeIndex++)
				{
					if (BuildSettingsScreen.RetrievedBuildSettings.SupportedBuildMode(buildModes[modeIndex]))
					{
						Progress = Mathf.Lerp(0.1f, 1.0f, modeIndex / (float)buildModes.Count);
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
							ShowProgress();
							foreach (var bundleFile in bundleBuilds.GetFiles())
							{
								if (bundleFile.Extension == "" && !bundleFile.Name.Contains("BundleBuilds"))
								{
									EmbedResourceCMD.EmbedResource(builtAssemblyPath, bundleFile.FullName, bundleFile.Name + mode.Extension, compression: WeaverBuildTools.Enums.CompressionMethod.NoCompression);
								}
							}
						}
						else
						{
							Debug.LogWarning(mode.Target.ToString() + " module is not loaded, so building for the target is not available");
						}
					}
				}
				ClearProgress();
			}
			finally
			{
				foreach (var registry in registries)
				{
					AdjustMonoScripts("Assembly-CSharp");
					AdjustMonoScripts("HollowKnight", path => path.EndsWith(".cs") && !path.Contains("Editor/") && path.Contains("Hollow Knight/"));
					registry.ReplaceAssemblyName(BuildSettingsScreen.RetrievedBuildSettings.ModName, "Assembly-CSharp");
					registry.ApplyChanges();
				}
			}

			//WeaverLog.Log("Launching Game");
			var hkEXE = new FileInfo(PathAddBackslash(BuildSettingsScreen.RetrievedBuildSettings.HollowKnightDirectory) + "hollow_knight.exe");


			if (hkEXE.FullName.Contains("steamapps"))
			{
				var steamDirectory = hkEXE.Directory;
				while (steamDirectory.Name != "Steam")
				{
					steamDirectory = steamDirectory.Parent;
					if (steamDirectory == null)
					{
						break;
					}
				}
				if (steamDirectory != null)
				{
					System.Diagnostics.Process.Start(steamDirectory.FullName + "\\steam.exe", "steam://rungameid/367520");
				}
				else
				{
					System.Diagnostics.Process.Start(hkEXE.FullName);
				}
			}
			else
			{
				System.Diagnostics.Process.Start(hkEXE.FullName);
			}
		}

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