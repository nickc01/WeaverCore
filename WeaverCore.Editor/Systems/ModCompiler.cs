using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverBuildTools.Commands;
using WeaverCore.Editor.Internal;
using WeaverCore.Editor.Structures;
using WeaverCore.Editor.Utilities;
using WeaverCore.Utilities;



namespace WeaverCore.Editor.Systems
{
	public static class ModCompiler
	{
		static string AsmLocationRelative = "Assets\\WeaverCore\\WeaverCore.Editor";
		static DirectoryInfo AsmEditorLocation = new DirectoryInfo("Assets\\WeaverCore\\WeaverCore.Editor");


		public static void BuildMod(string compileLocation = null, BuildSettings settings = null)
		{
			UnboundCoroutine.Start(BuildModAsync(compileLocation, settings));
		}

		public static void BuildWeaverCore(string compileLocation = null, BuildSettings settings = null)
		{
			UnboundCoroutine.Start(BuildWeaverCoreAsync(compileLocation, settings));
		}

		public static IEnumerator BuildModAsync(string compileLocation = null, BuildSettings settings = null)
		{
			if (LibraryCompiler.BuildingAssemblies)
			{
				yield return new WaitUntil(() => !LibraryCompiler.BuildingAssemblies);
			}
			if (settings == null)
			{
				settings = new BuildSettings();
				settings.GetStoredSettings();
			}
			if (compileLocation == null)
			{
				if (settings != null)
				{
					compileLocation = settings.BuildLocation;
				}
				else
				{
					compileLocation = SelectionUtilities.SelectFolder("Select where you want to place the finished mod", "");
					if (compileLocation == null)
					{
						throw new Exception("An invalid path specified for building the mod");
					}
				}
			}
			compileLocation = new DirectoryInfo(compileLocation).FullName;
			ProgressBar progress = new ProgressBar(7, "Compiling", "Compiling Mod : " + settings.ModName);

			try
			{
				WeaverReloadTools.DoReloadTools = false;
				WeaverReloadTools.DoOnScriptReload = false;

				progress.GoToNextStep();

				var mainModBuilder = new Builder
				{
					BuildPath = compileLocation + "\\" + settings.ModName + ".dll",
					Scripts = MonoScriptUtilities.GetScriptPathsUnderAssembly("Assembly-CSharp")
				};

				mainModBuilder.Defines.Add("GAME_BUILD");

				if (File.Exists(mainModBuilder.BuildPath))
				{
					File.Delete(mainModBuilder.BuildPath);
				}

				mainModBuilder.ReferencePaths.Add(LibraryCompiler.DefaultAssemblyCSharpLocation.FullName);
				mainModBuilder.ReferencePaths.Add(LibraryCompiler.DefaultWeaverCoreBuildLocation.FullName);
				mainModBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/WeaverCore.dll");
				mainModBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/Assembly-CSharp-Editor.dll");
				mainModBuilder.ExcludedReferences.Add("Editor");
				mainModBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/HollowKnight.dll");
				mainModBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/JUNK.dll");

				yield return mainModBuilder.Build();
				if (!mainModBuilder.BuildSuccessful)
				{
					throw new Exception("Error building mod. View console error messages for details");
				}
				progress.GoToNextStep();

				LibraryCompiler.BuildWeaverCore(compileLocation + "\\WeaverCore.dll");
				//yield return PrepareForBundling(settings.ModName);
				progress.GoToNextStep();
				List<BundleBuild> bundles = new List<BundleBuild>();
				//Debug.Log("BUILDING WEAVERCORE BUNDLES");
				yield return LibraryCompiler.BuildAssetBundles(bundles, settings.ModName, settings.GetBuildModes());
				foreach (var bundle in bundles)
				{
					//WeaverLog.Log("Bundle = " + bundle.File);
					if (bundle.File.Name.Contains(WeaverAssets.WeaverAssetBundleName))
					{
						WeaverLog.Log("Embedding Bundle into = " + compileLocation + "\\WeaverCore.dll");
						EmbedResourceCMD.EmbedResource(compileLocation + "\\WeaverCore.dll", bundle.File.FullName, bundle.File.Name + PlatformUtilities.GetBuildTargetExtension(bundle.Target), compression: WeaverBuildTools.Enums.CompressionMethod.NoCompression);
						progress.GoToNextStep();
					}
					else
					{
						//TODO - If the bundle is a part of weavercore, then embed it into weavercore instead
						EmbedResourceCMD.EmbedResource(mainModBuilder.BuildPath, bundle.File.FullName, bundle.File.Name + PlatformUtilities.GetBuildTargetExtension(bundle.Target), compression: WeaverBuildTools.Enums.CompressionMethod.NoCompression);
						progress.GoToNextStep();
					}
				}
				StartHollowKnight(settings);
			}
			finally
			{
				progress.Enabled = false;
				WeaverReloadTools.DoReloadTools = true;
				WeaverReloadTools.DoOnScriptReload = true;
				//AfterBundling(settings.ModName);
			}
			yield break;

		}

		public static IEnumerator BuildWeaverCoreAsync(string compileLocation = null, BuildSettings settings = null)
		{
			if (LibraryCompiler.BuildingAssemblies)
			{
				yield return new WaitUntil(() => !LibraryCompiler.BuildingAssemblies);
			}
			if (settings == null)
			{
				settings = new BuildSettings();
				settings.GetStoredSettings();
			}
			if (compileLocation == null)
			{
				if (settings != null)
				{
					compileLocation = settings.BuildLocation;
				}
				else
				{
					compileLocation = SelectionUtilities.SelectFolder("Select where you want to place the finished mod", "");
					if (compileLocation == null)
					{
						throw new Exception("An invalid path specified for building the mod");
					}
				}
			}
			compileLocation = new DirectoryInfo(compileLocation).FullName;
			ProgressBar progress = new ProgressBar(7, "Compiling", "Compiling WeaverCore");

			try
			{
				WeaverReloadTools.DoReloadTools = false;
				WeaverReloadTools.DoOnScriptReload = false;

				//progress.GoToNextStep();

				/*var mainModBuilder = new Builder
				{
					BuildPath = compileLocation + "\\" + "WeaverCore.dll",
					Scripts = MonoScriptUtilities.GetScriptPathsUnderAssembly("Assembly-CSharp")
				};*/

				/*if (File.Exists(mainModBuilder.BuildPath))
				{
					File.Delete(mainModBuilder.BuildPath);
				}

				mainModBuilder.ReferencePaths.Add(LibraryCompiler.DefaultAssemblyCSharpLocation.FullName);
				mainModBuilder.ReferencePaths.Add(LibraryCompiler.DefaultWeaverCoreBuildLocation.FullName);
				mainModBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/WeaverCore.dll");
				mainModBuilder.ExcludedReferences.Add("Editor");
				mainModBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/HollowKnight.dll");

				yield return mainModBuilder.Build();*/
				progress.GoToNextStep();

				LibraryCompiler.BuildWeaverCore(compileLocation + "\\WeaverCore.dll");
				//yield return PrepareForBundling(settings.ModName);
				progress.GoToNextStep();
				List<BundleBuild> bundles = new List<BundleBuild>();
				Debug.Log("BUILDING WEAVERCORE BUNDLES");
				yield return LibraryCompiler.BuildWeaverCoreBundles(bundles, settings.GetBuildModes());
				foreach (var bundle in bundles)
				{
					Debug.Log("Bundle = " + bundle.File);
					Debug.Log("Embedding into = " + compileLocation + "\\WeaverCore.dll");
					EmbedResourceCMD.EmbedResource(compileLocation + "\\WeaverCore.dll", bundle.File.FullName, bundle.File.Name + PlatformUtilities.GetBuildTargetExtension(bundle.Target), compression: WeaverBuildTools.Enums.CompressionMethod.NoCompression);
					progress.GoToNextStep();
				}
				StartHollowKnight(settings);
			}
			finally
			{
				progress.Enabled = false;
				WeaverReloadTools.DoReloadTools = true;
				WeaverReloadTools.DoOnScriptReload = true;
				//AfterBundling(settings.ModName);
			}
			yield break;

		}

		/*/// <summary>
		/// Builds all the asset bundles in the unity project. IMPORTANT: Make sure you call the 
		/// </summary>
		/// <param name="buildTargets">All the build targets to be built against</param>
		/// <returns>An iterator for each of the asset bundles built</returns>
		public static IEnumerable<BundleBuild> BuildAssetBundles(IEnumerable<BuildTarget> buildTargets)
		{
			var temp = Path.GetTempPath();
			var bundleBuilds = new DirectoryInfo(temp + "BundleBuilds\\");

			if (!bundleBuilds.Exists)
			{
				bundleBuilds.Create();
			}

			foreach (var target in buildTargets)
			{
				foreach (var existingFile in bundleBuilds.GetFiles())
				{
					existingFile.Delete();
				}
				BuildPipeline.BuildAssetBundles(bundleBuilds.FullName, BuildAssetBundleOptions.None, target);
				foreach (var bundleFile in bundleBuilds.GetFiles())
				{
					if (bundleFile.Extension == "" && !bundleFile.Name.Contains("BundleBuilds"))
					{
						yield return new BundleBuild()
						{
							File = bundleFile,
							Target = target
						};
					}
				}
			}
		}

		/// <summary>
		/// Prepares the Unity Project for bundling assets into asset bundles. This is required when building asset bundles with WeaverCore. IMPORTANT: Be sure to call <see cref="AfterBundling(string)"/> when you are done
		/// </summary>
		/// <param name="modName">The name of the mod that is to be bundled</param>
		/// <returns></returns>
		public static IEnumerator PrepareForBundling(string modName = null)
		{
			yield return SwitchToCompileMode();
			if (modName != null)
			{
				MonoScriptUtilities.ChangeAssemblyName("Assembly-CSharp", modName);
				foreach (var registry in RegistryChecker.LoadAllRegistries())
				{
					registry.ReplaceAssemblyName("Assembly-CSharp", modName);
					registry.ApplyChanges();
				}
			}
			MonoScriptUtilities.ChangeAssemblyName("HollowKnight", "Assembly-CSharp");
		}

		public static void AfterBundling(string modName = null)
		{
			SwitchToEditorMode();
			MonoScriptUtilities.ChangeAssemblyName("Assembly-CSharp", "HollowKnight");
			if (modName != null)
			{
				MonoScriptUtilities.ChangeAssemblyName(modName, "Assembly-CSharp");
				foreach (var registry in RegistryChecker.LoadAllRegistries())
				{
					registry.ReplaceAssemblyName(modName, "Assembly-CSharp");
					registry.ApplyChanges();
				}
			}
		}


		static IEnumerator SwitchToCompileMode()
		{
			//Debug.Log("ASMDEF LOcation = " + AsmEditorLocation.FullName + "\\WeaverCore.Editor.asmdef");
			if (!File.Exists(AsmEditorLocation.FullName + "\\WeaverCore.Editor-ORIGINAL.txt"))
			{
				File.Move(AsmEditorLocation.FullName + "\\WeaverCore.Editor.asmdef", AsmEditorLocation.FullName + "\\WeaverCore.Editor-ORIGINAL.txt");
				File.Move(AsmEditorLocation.FullName + "\\WeaverCore.Editor-COMPILEVERSION.txt", AsmEditorLocation.FullName + "\\WeaverCore.Editor.asmdef");
				AssetDatabase.ImportAsset(AsmLocationRelative + "\\WeaverCore.Editor.asmdef");
				yield return null;
			}
		}

		static void SwitchToEditorMode()
		{
			//Debug.Log("Switching BACK");
			if (File.Exists(AsmEditorLocation.FullName + "\\WeaverCore.Editor-ORIGINAL.txt"))
			{
				File.Move(AsmEditorLocation.FullName + "\\WeaverCore.Editor.asmdef", AsmEditorLocation.FullName + "\\WeaverCore.Editor-COMPILEVERSION.txt");
				File.Move(AsmEditorLocation.FullName + "\\WeaverCore.Editor-ORIGINAL.txt", AsmEditorLocation.FullName + "\\WeaverCore.Editor.asmdef");
				AssetDatabase.ImportAsset(AsmLocationRelative + "\\WeaverCore.Editor.asmdef");
			}
		}*/


		public static void StartHollowKnight(BuildSettings settings = null)
		{
			if (settings == null)
			{
				settings = BuildSettings.Retrieve<BuildSettings>();
			}
			if (settings.StartGame)
			{
				Debug.Log("<b>Starting Game</b>");
				var generalSettings = Settings.Retrieve<Settings>();
				var hkEXE = new FileInfo(generalSettings.HollowKnightDirectory + "\\hollow_knight.exe");


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
						return;
					}
				}
				System.Diagnostics.Process.Start(hkEXE.FullName);
			}
		}
	}
}
