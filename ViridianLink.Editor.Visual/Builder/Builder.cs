using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using ViridianLink.Editor.Helpers;
using ViridianLink.Editor.Routines;
using ViridianLink.Helpers;

namespace ViridianLink.Editor.Visual
{
	public static class Builder
	{
		const string asmDefFile = "{\n\"name\": \"def\"\n}\n";

		static float progress = 0.0f;

		static string ModName = "";
		static float Progress
		{
			get => progress;
			set
			{
				progress = value;
				EditorUtility.DisplayProgressBar("Compiling", "Compiling Mod : " + ModName, progress);
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

		[MenuItem("ViridianLink/Compile %F5")]
		public static void Compile()
		{
			ModNameSelector.ChooseString((modName) =>
			{
				EditorRoutine.Start(BeginCompile(modName));
			});
		}

		public static void ClearLogConsole()
		{
			var assembly = System.Reflection.Assembly.GetAssembly(typeof(SceneView));
			Type logEntries = assembly.GetType("UnityEditor.LogEntries");
			MethodInfo clearConsoleMethod = logEntries.GetMethod("Clear");
			clearConsoleMethod.Invoke(new object(), null);
		}

		static string SelectSaveLocation(string filename)
		{
			var binFolder = new DirectoryInfo("ViridianLink/bin").FullName;
			if (!Directory.Exists(binFolder))
			{
				Directory.CreateDirectory(binFolder);
			}
			return EditorUtility.SaveFilePanel("Select where you want to compile the mod", binFolder, filename, "dll");
		}

		static string[] GetFiles(string filter)
		{
			var EditorDirectory = new DirectoryInfo("Assets\\Editor").FullName;
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

		public static IEnumerator<IEditorWaiter> BeginCompile(string modName)
		{
			bool doneCompiling = false;
			string buildDestination = "";
			UnityEditor.Compilation.CompilerMessage[] messages = null;

			string destination = SelectSaveLocation(modName);

			if (destination == "")
			{
				yield break;
			}

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

		delegate void embedMethod(string sourceAssembly,string additionFile,string resourcePath,bool deleteSource);
		//Loads the resource embedder and returns a method to execute it
		static embedMethod LoadResourceEmbedder()
		{
			//Find the Assembly Loader Type
			var assemblyLoader = typeof(Builder).Assembly.GetType("Costura.AssemblyLoader");

			//Find the method that resolves the assembly
			var resolver = assemblyLoader.GetMethod("ResolveAssembly", BindingFlags.Public | BindingFlags.Static);

			//Resolve the assembly. Mono.Cecil should be loaded beyond this point
			resolver.Invoke(null, new object[] { null, new ResolveEventArgs("Mono.Cecil, Version=0.10.4.0, Culture=neutral, PublicKeyToken=50cebf1cceb9d05e") });

			//Load the resource embedder now that mono.cecil is loaded
			var embedder = AppDomain.CurrentDomain.Load("ResourceEmbedder");
			//Find the main program type
			var program = embedder.GetType("ResourceEmbedder.Program");

			//Find the main method
			var mainMethod = program.GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic);

			//Create a delegate to the main method
			var mainMethodRaw = (Func<string[],int>)Delegate.CreateDelegate(typeof(Func<string[],int>), null, mainMethod);

			return (source,addition,resourcePath,deleteSource) =>
			{
				mainMethodRaw(new string[] { source,addition,resourcePath,deleteSource.ToString()});
			};
		}


		static void PostBuild(string buildPath)
		{

			var Embed = LoadResourceEmbedder();
			var temp = Path.GetTempPath();
			for (int modeIndex = 0; modeIndex < buildModes.Count; modeIndex++)
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
					foreach (var newFile in bundleBuilds.GetFiles())
					{
						if (newFile.Extension == "")
						{
							Embed(buildPath, newFile.FullName, newFile.Name + mode.Extension, false);
						}
					}
				}
				else
				{
					Debug.LogWarning($"{mode.Target} module is not loaded, so building for the target is not available");
				}
			}
			ClearProgress();
			Debug.Log("Build Complete");
		}

		struct BuildMode
		{
			public string Extension;
			public BuildTarget Target;
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