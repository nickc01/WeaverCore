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
using WeaverCore.Editor;
using WeaverCore.Editor.Systems;
using WeaverCore.Editor.Utilities;
using WeaverCore.Utilities;

namespace WeaverAssets
{
	public static class BuildBundle
	{
		static readonly DirectoryInfo WeaverAssetsBuildLocation = new DirectoryInfo("..\\..\\WeaverAssets\\Bundles");

		[MenuItem("Weaver Assets/Build %F5")]
		public static void Build()
		{
			//BeginCompile();
			UnboundCoroutine.Start(BuildBundles());
		}


		static IEnumerator BuildBundles()
		{
			try
			{
				yield return ModCompiler.PrepareForBundling();
				foreach (var bundle in ModCompiler.BuildAssetBundles(PlatformUtilities.GetPCBuildTargets()))
				{
					if (bundle.File.Extension == "" && !bundle.File.Name.Contains("BundleBuilds"))
					{
						File.Copy(bundle.File.FullName, WeaverAssetsBuildLocation.FullName + "\\weavercore_bundle" + PlatformUtilities.GetBuildTargetExtension(bundle.Target), true);
					}
				}
				try
				{
					MonoScriptUtilities.ChangeAssemblyName("Assembly-CSharp", "HollowKnight");
					foreach (var bundle in ModCompiler.BuildAssetBundles(PlatformUtilities.GetPCBuildTargets()))
					{
						if (bundle.File.Extension == "" && !bundle.File.Name.Contains("BundleBuilds"))
						{
							File.Copy(bundle.File.FullName, WeaverAssetsBuildLocation.FullName + "\\weavercore_bundle_editor" + PlatformUtilities.GetBuildTargetExtension(bundle.Target), true);
						}
					}
				}
				finally
				{
					MonoScriptUtilities.ChangeAssemblyName("HollowKnight", "Assembly-CSharp");
				}
			}
			finally
			{
				ModCompiler.AfterBundling();
			}
		}

		/*public static void BeginCompile()
		{
			try
			{
				MonoScriptUtilities.ChangeAssemblyName("HollowKnight", "Assembly-CSharp");
				var weaverAssetsFolder = new DirectoryInfo(WeaverAssetsBuildLocation).FullName;
				var temp = Path.GetTempPath();

				foreach (var target in PlatformUtilities.GetPCBuildTargets())
				{
					var bundleBuilds = new DirectoryInfo(temp + @"BundleBuilds\");
					if (bundleBuilds.Exists)
					{
						bundleBuilds.Delete(true);
					}
					bundleBuilds.Create();
					BuildPipeline.BuildAssetBundles(bundleBuilds.FullName, BuildAssetBundleOptions.None, target);

					foreach (var bundleFile in bundleBuilds.GetFiles())
					{
						if (bundleFile.Extension == "" && !bundleFile.Name.Contains("BundleBuilds"))
						{
							File.Copy(bundleFile.FullName, weaverAssetsFolder + "\\weavercore_bundle" + PlatformUtilities.GetBuildTargetExtension(target), true);
						}
					}
				}
			}
			finally
			{
				MonoScriptUtilities.ChangeAssemblyName("Assembly-CSharp", "HollowKnight");
			}
		}*/
	}
}