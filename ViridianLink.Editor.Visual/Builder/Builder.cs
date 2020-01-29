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

		}

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


		static void PostBuild(string file)
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
			//}
			/*catch (TargetInvocationException e)
			{
				Debug.Log("Inner Exception = " + e.InnerException);
			}
			catch (Exception e)
			{
				Debug.LogError("EXCEPTION = " + e.Message);
				Debug.LogError("Type = " + e.GetType());
				Debug.LogError("Stack Trace = " + e.StackTrace);
			}*/
		}

		/*private static System.Reflection.Assembly CurrentDomain_TypeResolve(object sender, ResolveEventArgs args)
		{
			Debug.Log("Type = " + args.Name);
			return null;
		}*/

		/*private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			Debug.Log("Assembly to Load = " + args.Name);
			return null;
		}*/
	}

}