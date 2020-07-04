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
using WeaverCore.Editor.Visual.Helpers;
using WeaverCore.Editor.Visual.Internal;
using WeaverCore.Utilities;
using WeaverCore.Interfaces;
using WeaverCore.Internal;
using UnityEditor.PackageManager;

namespace WeaverCore.Editor.Visual
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


	public static class Builder
	{
		//static Assembly AssemblyManipulator;

		//delegate void embedMethod(string mode, string sourceAssembly, string additionFile, string resourcePath, bool compress);
		//delegate void addModMethod(string assembly, string @namespace, string typeName, string modName, bool unloadable, string hollowKnightPath,string weaverCorePath );
		//delegate void replaceTypesMethod(string assemblyWithTypes, string sourceAssembly, List<string> assembliesToLook);

		//static embedMethod Embed;
		//static replaceTypesMethod ReplaceTypes;
		//static addModMethod AddMod;

		static float progress = 0.0f;

		static bool lastBuildSuccessful = false;
		static string lastBuildDestination;

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

		static List<string> GetFiles(string filter)
		{
			var EditorDirectory = new DirectoryInfo("Assets\\WeaverCore").FullName;
			var AssetsFolder = new DirectoryInfo("Assets").FullName;
			var files = Directory.GetFiles(AssetsFolder, filter, SearchOption.AllDirectories).ToList();
			for (int i = files.Count - 1; i >= 0; i--)
			{
				if (files[i].Contains("\\Editor\\") || files[i].Contains("/Editor/") || files[i].Contains("\\Internal.cs") || files[i].Contains("_NOBUILD_"))
				{
					files.RemoveAt(i);
				}
			}
			return files;
		}

		static List<string> GetNoBuildFiles(string filter)
		{
			var EditorDirectory = new DirectoryInfo("Assets\\WeaverCore").FullName;
			var AssetsFolder = new DirectoryInfo("Assets").FullName;
			var files = Directory.GetFiles(AssetsFolder, filter, SearchOption.AllDirectories).ToList();
			for (int i = files.Count - 1; i >= 0; i--)
			{
				if (!(files[i].Contains("_NOBUILD_") && !(files[i].Contains("\\Editor\\") || files[i].Contains("/Editor/") || files[i].Contains("\\Internal.cs"))))
				{
					files.RemoveAt(i);
				}
				/*if ((!files[i].Contains("_NOBUILD_")) && (files[i].Contains("\\Editor\\") || files[i].Contains("/Editor/")))
				{
					files.RemoveAt(i);
				}*/
			}
			return files;
		}

		static string[] GetScripts(params string[] additions)
		{
			var files = GetFiles("*.cs");
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

			//return GetFiles("*.cs").AddRange(additions).ToArray();
		}

		static string[] GetNoBuildScripts(params string[] additions)
		{
			//return GetNoBuildFiles("*.cs").ToArray();
			var files = GetNoBuildFiles("*.cs");
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

		static string[] GetReferences(params string[] additions)
		{
			//return GetFiles("*.dll").ToArray();
			var files = GetFiles("*.dll");
			if (additions != null)
			{
				//files.AddRange(additions);
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

		static IEnumerable<IWeaverAwaiter> BuildAssembly(string destination, string[] scripts, string[] references)
		{
			lastBuildDestination = "";
			lastBuildSuccessful = false;
			bool doneCompiling = false;
			string buildDestination = "";
			UnityEditor.Compilation.CompilerMessage[] messages = null;

			var builder = new UnityEditor.Compilation.AssemblyBuilder(destination, scripts);

			builder.buildTarget = BuildTarget.StandaloneWindows;

			if (references != null)
			{
				builder.additionalReferences = references;
			}

			WeaverLog.Log("Build Dest = " + destination);
			if (scripts != null)
			{
				foreach (var script in scripts)
				{
					WeaverLog.Log("Script = " + script);
				}
			}

			builder.buildTargetGroup = BuildTargetGroup.Standalone;

			Action<string, UnityEditor.Compilation.CompilerMessage[]> finish = null;
			finish = (dest, m) =>
			{
				buildDestination = dest;
				messages = m;
				doneCompiling = true;
			};

			builder.buildFinished += finish;
			builder.Build();

			yield return new WaitTillTrue(() => doneCompiling);
			yield return null;

			bool errors = false;
			foreach (var message in messages)
			{
				switch (message.type)
				{
					case UnityEditor.Compilation.CompilerMessageType.Error:
						Debug.LogError(message.message);
						errors = true;
						break;
					case UnityEditor.Compilation.CompilerMessageType.Warning:
						Debug.LogWarning(message.message);
						break;
				}
			}
			builder.buildFinished -= finish;
			ClearProgress();

			lastBuildSuccessful = !errors;
			lastBuildDestination = buildDestination;
			/*if (errors)
			{
				lastBuildSuccessful = false;
				yield break;
			}
			lastBuildSuccessful = true;*/
		}

		public static IEnumerator<IWeaverAwaiter> BeginCompile()
		{
			//Debugger.Log("A");
			yield return BuildSettingsScreen.RetrieveBuildSettings();

			//Debugger.Log("B");
			if (BuildSettingsScreen.RetrievedBuildSettings == null)
			{
				yield break;
			}

			//bool doneCompiling = false;
			//string buildDestination = "";
			//UnityEditor.Compilation.CompilerMessage[] messages = null;

			//string destination = SelectSaveLocation(modName);

			var folder = SelectSaveLocation();
			if (folder == null)
			{
				yield break;
			}

			//string modFileName = new FileInfo(destination).Name.Replace(".dll", "");
			var destination = PathAddBackslash(folder.FullName) + BuildSettingsScreen.RetrievedBuildSettings.ModName + ".dll";
			//Debugger.Log("Mod File Name = " + destination);
			Progress = 0.0f;

			/*var scripts = GetNoBuildScripts();
			foreach (var script in scripts)
			{
				Debugger.Log("NO Script = " + script);
			}*/

			var noBuildTempDest = Path.GetTempPath() + @"Assembly-CSharp.dll";
			var noBuildDest = Path.GetTempPath() + @"no_build.dll";

			var noBuildScripts = GetNoBuildScripts();

			if (noBuildScripts == null || noBuildScripts.GetLength(0) == 0)
			{
				noBuildScripts = null;
			}
			else
			{
				foreach (var waiter in BuildAssembly(noBuildTempDest, GetNoBuildScripts(), null))
				{
					yield return waiter;
				}

				File.Copy(noBuildTempDest, noBuildDest, true);

				yield return null;
			}


			foreach (var waiter in BuildAssembly(destination,GetScripts(),GetReferences(noBuildScripts != null ? noBuildDest : "")))
			{
				yield return waiter;
			}

			Progress = 0.1f;

			if (!lastBuildSuccessful)
			{
				yield break;
			}

			var modBuildDestination = lastBuildDestination;

			yield return null;



			/*var builder = new UnityEditor.Compilation.AssemblyBuilder(destination, GetScripts());



			builder.buildTarget = BuildTarget.StandaloneWindows;

			builder.additionalReferences = GetReferences();

			builder.buildTargetGroup = BuildTargetGroup.Standalone;

			Action<string, UnityEditor.Compilation.CompilerMessage[]> finish = null;
			finish = (dest, m) =>
			{
				buildDestination = dest;
				messages = m;
				doneCompiling = true;
			};

			builder.buildFinished += finish;
			builder.Build();

			yield return new WaitTillTrue(() => doneCompiling);
			yield return null;

			bool errors = false;
			foreach (var message in messages)
			{
				switch (message.type)
				{
					case UnityEditor.Compilation.CompilerMessageType.Error:
						Debug.LogError(message.message);
						errors = true;
						break;
					case UnityEditor.Compilation.CompilerMessageType.Warning:
						Debug.LogWarning(message.message);
						break;
				}
			}
			builder.buildFinished -= finish;
			ClearProgress();
			if (errors)
			{
				yield break;
			}*/


			//TEMPORARY
			//System.Diagnostics.Process.Start("explorer.exe", buildDestination);

			//PostBuild(buildDestination);

			/*var noBuildTempDest = Path.GetTempPath() + @"no_build_temp.dll";
			var noBuildDest = Path.GetTempPath() + @"no_build.dll";

			foreach (var waiter in BuildAssembly(noBuildTempDest,GetNoBuildScripts(),null))
			{
				yield return waiter;
			}

			File.Copy(noBuildTempDest, noBuildDest,true);*/

			/*if (!lastBuildSuccessful)
			{
				yield break;
			}

			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;*/

			//var testAssembly = Assembly.Load("Assembly-CSharp");
			//Debugger.Log("Test Assembly = " + testAssembly);
			//var kerningTableTest = testAssembly.GetType("TMPro.KerningTable");
			//Debugger.Log("Kerning Table Test = " + kerningTableTest);
			//Debugger.Log("Build Destination = " + modBuildDestination);
			//Debugger.Log("Build Settings = " + BuildSettingsScreen.RetrievedBuildSettings);
			//Debugger.Log("Hollow Knight Directory = " + BuildSettingsScreen.RetrievedBuildSettings.HollowKnightDirectory);

			//LoadAssemblyManipulator();
			//ReplaceTypes(noBuildDest, modBuildDestination, new List<string>() {BuildSettingsScreen.RetrievedBuildSettings.HollowKnightDirectory + @"hollow_knight_Data\Managed\Assembly-CSharp.dll" });
			PostBuild(modBuildDestination);
		}
		
		//Loads the resource embedder and returns a method to execute it
		/*static void LoadAssemblyManipulator()
		{
			if (AssemblyManipulator == null)
			{
				//Find the Assembly Loader Type
				var assemblyLoader = typeof(Builder).Assembly.GetType("Costura.AssemblyLoader");

				//Find the method that resolves the assembly
				var resolver = assemblyLoader.GetMethod("ResolveAssembly", BindingFlags.Public | BindingFlags.Static);

				//Resolve the assembly. Mono.Cecil should be loaded beyond this point
				resolver.Invoke(null, new object[] { null, new ResolveEventArgs("Mono.Cecil, Version=0.10.4.0, Culture=neutral, PublicKeyToken=50cebf1cceb9d05e") });

				//Load the resource embedder now that mono.cecil is loaded
				AssemblyManipulator = AppDomain.CurrentDomain.Load("WeaverBuildTools");
				//Find the main program type
				var program = AssemblyManipulator.GetType("AssemblyManipulator.Program");

				//Find the main method
				var mainMethod = program.GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic);

				//Create a delegate to the main method
				var mainMethodRaw = (Func<string[], int>)Delegate.CreateDelegate(typeof(Func<string[], int>), null, mainMethod);

				Embed = (mode, source, addition, resourcePath, compress) =>
				{
					mainMethodRaw(new string[] { mode, source, addition, resourcePath, compress.ToString() });
				};

				var addModM = program.GetMethod("AddMod",BindingFlags.NonPublic | BindingFlags.Static);

				AddMod = (addModMethod)Delegate.CreateDelegate(typeof(addModMethod), null, addModM);

				var replaceTypesM = program.GetMethod("ReplaceTypes", BindingFlags.Public | BindingFlags.Static);

				ReplaceTypes = (replaceTypesMethod)Delegate.CreateDelegate(typeof(replaceTypesMethod), null, replaceTypesM);
			}
		}*/

		/*class DebugConsole : TextWriter
		{
			public override Encoding Encoding => Encoding.ASCII;

			public override void WriteLine(object value)
			{
				Debugger.Log(value);
			}
			public override void WriteLine(string value)
			{
				Debugger.Log(value);
			}
			public override void Write(object value)
			{
				Debugger.Log(value);
			}
			public override void Write(string value)
			{
				Debugger.Log(value);
			}
		}*/

		/*static Dictionary<string,string> GetScriptAssemblyNames()
		{
			Dictionary<string, string> assemblyNames = new Dictionary<string, string>();

			IEnumerable<string> assetFolderPaths = AssetDatabase.GetAllAssetPaths().Where(path => path.EndsWith(".cs") && !path.Contains("Assets/Editor") && !path.Contains("_NOBUILD_"));

			foreach (var path in assetFolderPaths)
			{
				MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

				if (script != null)
				{
					assemblyNames.Add(script,)
					//ChangeMonoScriptAssembly(script, newAssemblyName);
				}
			}
		}*/


		static void AdjustMonoScripts(string newAssemblyName)
		{
			//Debug.Log("Adjusting scripts...");

			IEnumerable<string> assetFolderPaths = AssetDatabase.GetAllAssetPaths().Where(path => path.EndsWith(".cs") && !path.Contains("Assets/Editor") && !path.Contains("_NOBUILD_"));

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
			//List<Type> ValidMods = Assembly.Load("Assembly-CSharp").GetTypes().Where(type => typeof(IWeaverMod).IsAssignableFrom(type) && !type.IsAbstract && !type.IsGenericTypeDefinition && !type.IsInterface && !typeof(WeaverCoreMod).IsAssignableFrom(type)).ToList();
			AdjustMonoScripts(BuildSettingsScreen.RetrievedBuildSettings.ModName);
			//LoadAssemblyManipulator();
			var temp = Path.GetTempPath();
			var registries = RegistryChecker.LoadAllRegistries();
			try
			{
				foreach (var registry in registries)
				{
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
									//PostProcessBundle(builtAssemblyPath, bundleFile.FullName, bundleFile.Name + mode.Extension,bundleFile.Name);
									//Embed("addresource", builtAssemblyPath, bundleFile.FullName, bundleFile.Name + mode.Extension, false);
									//BuildTools.EmbedResource(builtAssemblyPath, bundleFile.FullName, bundleFile.Name + mode.Extension, false);
									BuildTools.EmbedResource(builtAssemblyPath, bundleFile.FullName, bundleFile.Name + mode.Extension);
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
				foreach (var modType in ValidMods)
				{
					var instance = Activator.CreateInstance(modType) as IWeaverMod;
					//Debugger.Log("builtAssemblyPath = " + builtAssemblyPath);
					//Debugger.Log("WeaverCore Location = " + typeof(WeaverCore.Internal.WeaverCore).Assembly.Location);
					//AddMod(builtAssemblyPath, modType.Namespace, modType.Name, instance.Name, instance.Unloadable, BuildSettingsScreen.RetrievedBuildSettings.HollowKnightDirectory,typeof(WeaverCore.Internal.WeaverCore).Assembly.Location);
					BuildTools.AddMod(builtAssemblyPath, modType.Namespace, modType.Name, instance.Name, instance.Unloadable, BuildSettingsScreen.RetrievedBuildSettings.HollowKnightDirectory, typeof(WeaverCore.Internal.WeaverCore).Assembly.Location);
				}
				Debug.Log("Build Complete");
			}
			finally
			{
				foreach (var registry in registries)
				{
					AdjustMonoScripts("Assembly-CSharp");
					registry.ReplaceAssemblyName(BuildSettingsScreen.RetrievedBuildSettings.ModName, "Assembly-CSharp");
					registry.ApplyChanges();
					//Debugger.Log("Registry Mod Assembly Name Old = " + registry.GetString("modAssemblyName"));
				}
			}

			var hkEXE = PathAddBackslash(BuildSettingsScreen.RetrievedBuildSettings.HollowKnightDirectory) + "hollow_knight.exe";

			if (File.Exists(hkEXE))
			{
				System.Diagnostics.Process.Start(hkEXE);
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