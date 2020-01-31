using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
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

		static void ClearProgress()
		{
			EditorUtility.ClearProgressBar();
		}

		[MenuItem("ViridianLink/Compile %F5")]
		public static void Compile()
		{
			ModNameSelector.ChooseString(BeginCompile);
		}

		public static void ClearLogConsole()
		{
			var assembly = System.Reflection.Assembly.GetAssembly(typeof(SceneView));
			Debug.Log("Assembly = " + assembly?.FullName);
			Type logEntries = assembly.GetType("UnityEditor.LogEntries");
			Debug.Log("Type = " + logEntries?.Name);
			MethodInfo clearConsoleMethod = logEntries.GetMethod("Clear");
			Debug.Log("Method = " + clearConsoleMethod?.Name);
			clearConsoleMethod.Invoke(new object(), null);
		}

		static void BeginCompile(string modName)
		{
			try
			{
				ClearLogConsole();
				var binFolder = new DirectoryInfo("ViridianLink/bin").FullName;
				if (!Directory.Exists(binFolder))
				{
					Directory.CreateDirectory(binFolder);
				}
				string destination = EditorUtility.SaveFilePanel("Select where you want to compile the mod", binFolder, PlayerSettings.productName.Replace(" ", ""), "dll");
				if (destination == "")
				{
					return;
				}
				Debug.Log("Beginning Compilation");
				AssetDatabase.StartAssetEditing();
				var guids = AssetDatabase.FindAssets("ModName");
				if (guids != null && guids.GetLength(0) > 0)
				{
					foreach (var guid in guids)
					{
						Debug.Log("B");
						var path = AssetDatabase.GUIDToAssetPath(guid);
						Debug.Log("Asset Path = " + path);
						Debug.Log("A");
						AssetDatabase.DeleteAsset(path);
					}
				}
				//Debug.Log("Path = " + );
				var asmPath = new DirectoryInfo("Assets").FullName + "\\ModName.asmdef";
				using (var writer = File.CreateText(asmPath))
				{
					writer.Write(asmDefFile.Replace("def", modName));
				}
				AssetDatabase.StopAssetEditing();
				AssetDatabase.ImportAsset("Assets\\ModName.asmdef");
				var temp = Path.GetTempPath();
				var errors = BuildPipeline.BuildPlayer(new string[] { }, temp + "\\build\\build.exe", BuildTarget.StandaloneWindows, BuildOptions.BuildScriptsOnly);
				Debug.Log("Erros = " + errors);

				//ClearLogConsole();

				var buildPath = temp + "\\build\\build_Data\\Managed\\" + modName + ".dll";

				//TODO -- BUILD THE ASSET BUNDLES AND EMBED THEM INTO THE ASSEMBLY. THEN COPY THE ASSEMBLY TO THE DESTINATION

				List<BuildMode> modes = new List<BuildMode>()
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
				};

				var assemblyLoader = typeof(Builder).Assembly.GetType("Costura.AssemblyLoader");

				var resolver = assemblyLoader.GetMethod("ResolveAssembly", BindingFlags.Public | BindingFlags.Static);

				var result = (System.Reflection.Assembly)resolver.Invoke(null, new object[] { null, new ResolveEventArgs("Mono.Cecil, Version=0.10.4.0, Culture=neutral, PublicKeyToken=50cebf1cceb9d05e") });

				Debugger.Log("Result = " + result.FullName);

				var embedder = AppDomain.CurrentDomain.Load("ResourceEmbedder");
				var program = embedder.GetType("ResourceEmbedder.Program");
				var Main = program.GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic);

				foreach (var name in AssetDatabase.GetAllAssetBundleNames())
				{
					foreach (var mode in modes)
					{
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
							foreach (var newFile in bundleBuilds.GetFiles())
							{
								if (newFile.Extension == "")
								{
									Main.Invoke(null, new object[] { new string[] { buildPath, newFile.FullName, newFile.Name + mode.Extension, "false" } });
								}
							}
						}
						else
						{
							Debug.LogWarning($"{mode.Target} module is not loaded, so building for the target is not available");
						}
					}
				}

				File.Copy(buildPath, destination, true);
				//ClearLogConsole();

				//TODO -- THEN, FIGURE OUT HOW TO RELOAD THE ASSEMBLY CONSISTENTLY

			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
		}

		/*[MenuItem("ViridianLink/CompileOLD")]
		public static void CompileOLD()
		{
			try
			{

				var binFolder = new DirectoryInfo("ViridianLink/bin").FullName;
				if (!Directory.Exists(binFolder))
				{
					Directory.CreateDirectory(binFolder);
				}
				string destination = EditorUtility.SaveFilePanel("Select where you want to compile the mod", binFolder, PlayerSettings.productName.Replace(" ", ""), "dll");
				if (destination == "")
				{
					return;
				}
				Debug.Log("New Destination = " + destination);
				Debug.Log("Compiling");
				var assetsFolder = new DirectoryInfo("Assets").FullName;

				var scripts = Directory.GetFiles(assetsFolder, "*.cs", SearchOption.AllDirectories);
				var references = Directory.GetFiles(assetsFolder, "*.dll", SearchOption.AllDirectories);

				Progress = 0.0f;

				AssemblyBuilder builder = new AssemblyBuilder(destination, scripts);

				builder.buildTarget = BuildTarget.StandaloneWindows;

				builder.additionalReferences = references;

				builder.buildTargetGroup = BuildTargetGroup.Standalone;

				Action<string, CompilerMessage[]> finish = null;
				finish = (dest, messages) =>
				{
					Debug.Log("FINISHED");
					bool errors = false;
					foreach (var message in messages)
					{
						switch (message.type)
						{
							case CompilerMessageType.Error:
								Debug.LogError(message.message);
								errors = true;
								break;
							case CompilerMessageType.Warning:
								Debug.LogWarning(message.message);
								break;
						}
					}
					builder.buildFinished -= finish;
					Progress = 1.0f;
					ClearProgress();
					if (!errors)
					{
						PostBuild(dest);
					}
				};

				builder.buildFinished += finish;
				builder.Build();
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}

		}*/

		struct BuildMode
		{
			public string Extension;
			public BuildTarget Target;
		}


		public static bool IsPlatformSupportLoaded(BuildTarget buildTarget)
		{
			var UnityEditor = System.Reflection.Assembly.Load("UnityEditor");	
			var ModuleManagerT = UnityEditor.GetType("UnityEditor.Modules.ModuleManager");

			var buildString = (string)ModuleManagerT.GetMethod("GetTargetStringFromBuildTarget", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { buildTarget });
			return (bool)ModuleManagerT.GetMethod("IsPlatformSupportLoaded", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { buildString });

		}


		/*static void PostBuild(string file)
		{
			List<BuildMode> modes = new List<BuildMode>()
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
			};

			var assemblyLoader = typeof(Builder).Assembly.GetType("Costura.AssemblyLoader");

			var resolver = assemblyLoader.GetMethod("ResolveAssembly",BindingFlags.Public | BindingFlags.Static);

			var result = (System.Reflection.Assembly)resolver.Invoke(null, new object[] { null,new ResolveEventArgs("Mono.Cecil, Version=0.10.4.0, Culture=neutral, PublicKeyToken=50cebf1cceb9d05e") });

			Debugger.Log("Result = " + result.FullName);

			var embedder = AppDomain.CurrentDomain.Load("ResourceEmbedder");
			var program = embedder.GetType("ResourceEmbedder.Program");
			var Main = program.GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic);

			foreach (var name in AssetDatabase.GetAllAssetBundleNames())
			{
				foreach (var mode in modes)
				{
					if (IsPlatformSupportLoaded(mode.Target))
					{
						var temp = Path.GetTempPath();
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
						foreach (var newFile in bundleBuilds.GetFiles())
						{
							if (newFile.Extension == "")
							{
								Main.Invoke(null, new object[] { new string[] { file, newFile.FullName, newFile.Name + mode.Extension, "false" } });
							}
						}
					}
					else
					{
						Debug.LogWarning($"{mode.Target} module is not loaded, so building for the target is not available");
					}
				}
			}
		}*/
	}

}