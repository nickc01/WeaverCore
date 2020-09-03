using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;
using WeaverBuildTools.Commands;
using WeaverBuildTools.Enums;
using WeaverCore.Editor.Internal;
using WeaverCore.Editor.Structures;
using WeaverCore.Editor.Utilities;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Systems
{
	public static class LibraryCompiler
	{
		public static bool BuildingAssemblies { get; private set; }
		public static bool BuildingBundles { get; private set; }

		static string AsmLocationRelative = "Assets\\WeaverCore\\WeaverCore.Editor";
		static DirectoryInfo AsmEditorLocation = new DirectoryInfo("Assets\\WeaverCore\\WeaverCore.Editor");

		public static IEnumerable<BuildTarget> MainTargets
		{
			get
			{
				if (PlatformUtilities.IsPlatformSupportLoaded(BuildTarget.StandaloneWindows))
				{
					yield return BuildTarget.StandaloneWindows;
				}

				if (PlatformUtilities.IsPlatformSupportLoaded(BuildTarget.StandaloneOSX))
				{
					yield return BuildTarget.StandaloneOSX;
				}

				if (PlatformUtilities.IsPlatformSupportLoaded(BuildTarget.StandaloneLinuxUniversal))
				{
					yield return BuildTarget.StandaloneLinuxUniversal;
				}
			}
		}


		class OnReload : IInit
		{
			void IInit.OnInit()
			{
				if (WeaverReloadTools.DoReloadTools)
				{
					BuildStrippedWeaverCore();
				}
			}
		}



		public static FileInfo DefaultWeaverCoreBuildLocation
		{
			get
			{
				return new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore Build\\WeaverCore.dll");
			}
		}
		public static FileInfo DefaultAssemblyCSharpLocation
		{
			get
			{
				return new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore Build\\Assembly-CSharp.dll");
			}
		}

		/// <summary>
		/// Builds both the HollowKnight assembly and a partial version of WeaverCore that does not contain any embedded resources. This is primarily used with WeaverCore.Game
		/// </summary>
		public static void BuildStrippedWeaverCore()
		{
			BuildStrippedWeaverCore(DefaultWeaverCoreBuildLocation.FullName);
		}

		public static void BuildStrippedWeaverCore(string buildLocation)
		{
			UnboundCoroutine.Start(BuildStrippedWeaverCoreAsync(buildLocation));
		}

		/// <summary>
		/// Builds a full version of WeaverCore that does contain embedded resources.
		/// </summary>
		/// <param name="buildLocation"></param>
		public static void BuildWeaverCore(string buildLocation)
		{

			File.Copy(DefaultWeaverCoreBuildLocation.FullName, buildLocation, true);

			var weaverGameLocation = new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore.Game\\bin\\WeaverCore.Game.dll");
			var harmonyLocation = new FileInfo("Assets\\WeaverCore\\Libraries\\0Harmony.dll");

			EmbedResourceCMD.EmbedResource(buildLocation, weaverGameLocation.FullName, "WeaverCore.Game", compression: CompressionMethod.NoCompression);
			EmbedResourceCMD.EmbedResource(buildLocation, harmonyLocation.FullName, "0Harmony", compression: CompressionMethod.NoCompression);
		}

		/// <summary>
		/// Builds only the bundles required for WeaverCore. The list will get populated when the function is done
		/// </summary>
		/// <param name="bundles"></param>
		/// <returns></returns>
		public static IEnumerator BuildWeaverCoreBundles(List<BundleBuild> bundles, IEnumerable<BuildTarget> buildTargets)
		{
			yield return BuildAssetBundles(bundles, null, buildTargets);
			for (int i = bundles.Count - 1; i >= 0; i--)
			{
				if (!bundles[i].File.Name.Contains(WeaverAssets.WeaverAssetBundleName))
				{
					bundles.RemoveAt(i);
				}
			}
		}

		/// <summary>
		/// Builds both the mod bundles and the WeaverCore Bundles. The list will get populated when the function is done
		/// </summary>
		/// <param name="bundles"></param>
		/// <param name="modName"></param>
		/// <returns></returns>
		public static IEnumerator BuildAssetBundles(List<BundleBuild> bundles, string modName, IEnumerable<BuildTarget> buildTargets)
		{
			if (BuildingBundles)
			{
				yield break;
			}
			if (BuildingAssemblies)
			{
				yield return new WaitUntil(() => !BuildingAssemblies);
			}
			try
			{
				BuildingAssemblies = true;
				BuildingBundles = true;
				WeaverLog.Log("Beginning Bundling");
				yield return PrepareForBundling(modName);
				var temp = Path.GetTempPath();
				var bundleBuilds = new DirectoryInfo(temp + "BundleBuilds\\");

				if (bundleBuilds.Exists)
				{
					bundleBuilds.Delete(true);
				}

				bundleBuilds.Create();

				foreach (var target in buildTargets)
				{
					var targetFolder = bundleBuilds.CreateSubdirectory(target.ToString());

					targetFolder.Create();
					BuildPipeline.BuildAssetBundles(targetFolder.FullName, BuildAssetBundleOptions.None, target);
					foreach (var bundleFile in targetFolder.GetFiles())
					{
						if (bundleFile.Extension == "" && !bundleFile.Name.Contains("BundleBuilds"))
						{
							bundles.Add(new BundleBuild() { File = bundleFile, Target = target});
						}
					}
				}
			}
			finally
			{
				BuildingAssemblies = false;
				BuildingBundles = false;
				AfterBundling(modName);
				WeaverLog.Log("Done Bundling");
			}
		}

		static IEnumerator PrepareForBundling(string modName = null)
		{
			AssemblyReplacer.AssemblyReplacements.Add("HollowKnight.dll", "Assembly-CSharp.dll");
			if (modName != null)
			{
				AssemblyReplacer.AssemblyReplacements.Add("Assembly-CSharp.dll", modName + ".dll");
				MonoScriptUtilities.ChangeAssemblyName("Assembly-CSharp", modName);
				foreach (var registry in RegistryChecker.LoadAllRegistries())
				{
					registry.ReplaceAssemblyName("Assembly-CSharp", modName);
					registry.ApplyChanges();
				}
			}
			MonoScriptUtilities.ChangeAssemblyName("HollowKnight", "Assembly-CSharp");
			yield return SwitchToCompileMode();
		}

		static void AfterBundling(string modName = null)
		{
			AssemblyReplacer.AssemblyReplacements.Clear();
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
			SwitchToEditorMode();
		}

		static IEnumerator SwitchToCompileMode()
		{
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
		}

		static IEnumerator BuildHollowKnightASM(string buildLocation)
		{
			var assemblyCSharpBuilder = new Builder();
			assemblyCSharpBuilder.BuildPath = buildLocation;
			var scripts = Builder.GetAllRuntimeInDirectory("*.cs", "Assets\\WeaverCore\\Hollow Knight");
			scripts.RemoveAll(f => f.FullName.Contains("Editor\\"));
			assemblyCSharpBuilder.Scripts = scripts;
			assemblyCSharpBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/HollowKnight.dll");
			if (File.Exists(buildLocation))
			{
				File.Delete(buildLocation);
			}

			yield return assemblyCSharpBuilder.Build();
		}

		/// <summary>
		/// Builds a partial version of WeaverCore and the HollowKnight assembly
		/// </summary>
		/// <param name="buildLocation">Where the build will be placed</param>
		/// <returns></returns>
		public static IEnumerator BuildStrippedWeaverCoreAsync(string buildLocation)
		{
			if (BuildingAssemblies)
			{
				yield return new WaitUntil(() => !BuildingAssemblies);
			}
			try
			{
				BuildingAssemblies = true;
				yield return BuildHollowKnightASM(DefaultAssemblyCSharpLocation.FullName);

				var weaverCoreBuilder = new Builder();
				weaverCoreBuilder.BuildPath = buildLocation;
				weaverCoreBuilder.Scripts = Builder.GetAllRuntimeInDirectory("*.cs", "Assets\\WeaverCore\\WeaverCore").Where(f => f.Directory.FullName.Contains(""));

				//For some reason, this only works when using forward slashes and not backslashes.
				weaverCoreBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/WeaverCore.dll");
				weaverCoreBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/HollowKnight.dll");

				weaverCoreBuilder.ReferencePaths.Add(DefaultAssemblyCSharpLocation.FullName);


				if (File.Exists(buildLocation))
				{
					File.Delete(buildLocation);
				}
				yield return weaverCoreBuilder.Build();
			}
			finally
			{
				BuildingAssemblies = false;
			}
		}
	}

	class AssemblyReplacer : SimplePatcher
	{
		public static Dictionary<string, string> AssemblyReplacements = new Dictionary<string, string>();

		static bool EnableReplacements
		{
			get
			{
				return AssemblyReplacements.Count > 0;
			}
		}

		protected override Type GetClassToPatch()
		{
			return typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");
		}

		public static void PostfixGetTargetAssemblies(ref Array __result)
		{
			try
			{
				if (EnableReplacements)
				{
					for (int i = 0; i < __result.GetLength(0); i++)
					{
						var asmInfo = __result.GetValue(i);

						var nameField = asmInfo.GetType().GetField("Name");

						var name = (string)nameField.GetValue(asmInfo);

						if (AssemblyReplacements.ContainsKey(name))
						{
							nameField.SetValue(asmInfo, AssemblyReplacements[name]);
							__result.SetValue(asmInfo, i);
						}
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogError("Error in GetTargetAssemblies: " + e);
			}
		}

		public static void PostfixGetTargetAssembly(ref object __result, string scriptPath)
		{
			try
			{
				if (__result is string)
				{
					var assembly = (string)__result;
					if (EnableReplacements && AssemblyReplacements.ContainsKey(assembly))
					{
						__result = AssemblyReplacements[assembly];
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogError("Error in GetTargetAssembly: " + e);
			}
		}
	}
}
