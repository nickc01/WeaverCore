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
using WeaverCore.Editor.Helpers;
using WeaverCore.Editor.Routines;
using WeaverCore.Editor.Visual.Helpers;
using WeaverCore.Editor.Visual.Internal;
using WeaverCore.Helpers;
using WeaverCore.Internal;

namespace WeaverCore.Editor.Visual
{
	public static class Builder
	{
		static Assembly AssemblyManipulator;

		delegate void embedMethod(string mode, string sourceAssembly, string additionFile, string resourcePath, bool compress);
		delegate void addModMethod(string assembly, string @namespace, string typeName, string modName, bool unloadable, string hollowKnightPath,string weaverCorePath );

		static embedMethod Embed;

		static addModMethod AddMod;

		static float progress = 0.0f;

		static float Progress
		{
			get => progress;
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
			EditorRoutine.Start(BeginCompile());
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
					buildFolder = new DirectoryInfo($"{nameof(WeaverCore)}/bin").FullName;
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

		static string[] GetFiles(string filter)
		{
			var EditorDirectory = new DirectoryInfo($"Assets\\{nameof(WeaverCore)}").FullName;
			var AssetsFolder = new DirectoryInfo("Assets").FullName;
			var files = Directory.GetFiles(AssetsFolder, filter, SearchOption.AllDirectories).ToList();
			for (int i = files.Count - 1; i >= 0; i--)
			{
				if (files[i].Contains(EditorDirectory))
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

		static string PathAddBackslash(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			path = path.TrimEnd();

			if (PathEndsWithDirectorySeparator())
				return path;

			return path + GetDirectorySeparatorUsedInPath();

			bool PathEndsWithDirectorySeparator()
			{
				if (path.Length == 0)
					return false;

				char lastChar = path[path.Length - 1];
				return lastChar == Path.DirectorySeparatorChar
					|| lastChar == Path.AltDirectorySeparatorChar;
			}

			char GetDirectorySeparatorUsedInPath()
			{
				if (path.Contains(Path.AltDirectorySeparatorChar))
					return Path.AltDirectorySeparatorChar;

				return Path.DirectorySeparatorChar;
			}
		}

		public static IEnumerator<IEditorWaiter> BeginCompile()
		{
			yield return BuildSettingsScreen.RetrieveBuildSettings();
			if (BuildSettingsScreen.RetrievedBuildSettings == null)
			{
				yield break;
			}

			bool doneCompiling = false;
			string buildDestination = "";
			UnityEditor.Compilation.CompilerMessage[] messages = null;

			//string destination = SelectSaveLocation(modName);

			var folder = SelectSaveLocation();
			if (folder == null)
			{
				yield break;
			}

			//string modFileName = new FileInfo(destination).Name.Replace(".dll", "");
			var destination = PathAddBackslash(folder.FullName) + BuildSettingsScreen.RetrievedBuildSettings.ModName + ".dll";
			Debugger.Log("Mod File Name = " + destination);

			/*using (var file = File.Create("Assets\\ModName.asmdef"))
			{
				using (var writer = new StreamWriter(file))
				{
					writer.Write(asmDefFile.Replace("def", modFileName));
				}
			}
			AssetDatabase.ImportAsset("Assets\\ModName.asmdef",ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);*/

			Progress = 0.0f;

			var builder = new UnityEditor.Compilation.AssemblyBuilder(destination, GetScripts());

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
			}
			Progress = 0.1f;
			PostBuild(buildDestination);
		}
		
		//Loads the resource embedder and returns a method to execute it
		static void LoadAssemblyManipulator()
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
				AssemblyManipulator = AppDomain.CurrentDomain.Load("AssemblyManipulator");
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
			}
		}

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


		static void AdjustMonoScripts()
		{
			//Debug.Log("Adjusting scripts...");

			IEnumerable<string> assetFolderPaths = AssetDatabase.GetAllAssetPaths().Where(path => path.EndsWith(".cs") && !path.Contains("Assets/Editor"));

			foreach (var path in assetFolderPaths)
			{
				MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

				if (script != null)
				{
					ChangeMonoScriptAssembly(script, BuildSettingsScreen.RetrievedBuildSettings.ModName);
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
					AddMod(builtAssemblyPath, modType.Namespace, modType.Name, instance.Name, instance.Unloadable, BuildSettingsScreen.RetrievedBuildSettings.HollowKnightDirectory,typeof(WeaverCore.Internal.WeaverCore).Assembly.Location);
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