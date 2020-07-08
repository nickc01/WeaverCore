using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.Compilation;
using UnityEngine;
using WeaverBuildTools.Commands;
using WeaverBuildTools.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Internal
{
	public static class WeaverCoreBuilder
	{
		class NewBuildOnReload : IInit
		{
			public void OnInit()
			{
				//Debug.Log("Building WEAVERCORE");
				if (!WeaverAssetsInfo.InWeaverAssetsProject)
				{
					Build();
				}
			}
		}




		static string defaultBuildLocation;
		public static string DefaultBuildLocation
		{ 
			get
			{
				if (defaultBuildLocation == null)
				{
					defaultBuildLocation = new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore Build\\WeaverCore.dll").FullName;
				}
				return defaultBuildLocation;
			}
		}

		static string defaultAssemblyCSharpLocation;
		public static string DefaultAssemblyCSharpLocation
		{
			get
			{
				if (defaultAssemblyCSharpLocation == null)
				{
					defaultAssemblyCSharpLocation = new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore Build\\Assembly-CSharp.dll").FullName;
				}
				return defaultAssemblyCSharpLocation;
			}
		}



		static void Build()
		{
			Build(DefaultBuildLocation);
		}

		static void Build(string buildLocation)
		{
			WeaverRoutine.Start(BuildAsync(buildLocation));
		}

		public static void BuildFinishedVersion(string buildLocation)
		{
			File.Copy(DefaultBuildLocation,buildLocation,true);


			var weaverAssetsLocation = new DirectoryInfo("Assets\\WeaverCore\\WeaverAssets\\Bundles").FullName;

			var windowsBundle = weaverAssetsLocation + "\\weavercore.bundle.win";
			var linuxBundle = weaverAssetsLocation + "\\weavercore.bundle.unix";
			var macBundle = weaverAssetsLocation + "\\weavercore.bundle.mac";
			var weaverGameLocation = new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore.Game\\bin\\WeaverCore.Game.dll");
			var harmonyLocation = new FileInfo("Assets\\WeaverCore\\Editor\\Harmony\\0Harmony.dll");

			EmbedResourceCMD.EmbedResource(buildLocation, windowsBundle, "weavercore.bundle.win", compression: CompressionMethod.NoCompression);
			EmbedResourceCMD.EmbedResource(buildLocation, macBundle, "weavercore.bundle.mac", compression: CompressionMethod.NoCompression);
			EmbedResourceCMD.EmbedResource(buildLocation, linuxBundle, "weavercore.bundle.unix", compression: CompressionMethod.NoCompression);
			EmbedResourceCMD.EmbedResource(buildLocation, weaverGameLocation.FullName, "WeaverCore.Game");
			EmbedResourceCMD.EmbedResource(buildLocation, harmonyLocation.FullName, "0Harmony");
		}


		static IEnumerator<IWeaverAwaiter> BuildAsync(string buildLocation)
		{
			//Debug.Log("IN BUILD");
			//Debug.Log("Assembly-CSharp Location = " + DefaultAssemblyCSharpLocation);
			//Build Assembly-CSharp
			var assemblyCSharpBuilder = new Builder();
			assemblyCSharpBuilder.BuildPath = DefaultAssemblyCSharpLocation;
			assemblyCSharpBuilder.Scripts = Builder.GetAllRuntimeInDirectory("*.cs", "Assets\\WeaverCore\\Hollow Knight");

			/*foreach (var file in assemblyCSharpBuilder.Scripts = Builder.GetAllRuntimeInDirectory("*.cs", "Assets\\WeaverCore\\Hollow Knight"))
			{
				Debug.Log("Hollow Knight File = " + file);
			}*/

			yield return assemblyCSharpBuilder.Build();

			//Debug.Log("IN BUILD 2");
			//Debug.Log("WeaverCore Build = " + buildLocation);
			//Build WeaverCore
			var weaverCoreBuilder = new Builder();
			weaverCoreBuilder.BuildPath = buildLocation;
			//weaverCoreBuilder.ScriptPaths.Add(new FileInfo("Assets\\test.cs").FullName);
			weaverCoreBuilder.Scripts = Builder.GetAllRuntimeInDirectory("*.cs", "Assets\\WeaverCore\\WeaverCore").Where(f => f.Directory.FullName.Contains(""));

			//For some reason, this only works when using forward slashes and not backslashes. Trust me, I even decompiled UnityEditor.dll and checked.
			weaverCoreBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/WeaverCore.dll");
			weaverCoreBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/HollowKnight.dll");
			/*foreach (var file in Builder.GetAllRuntimeInDirectory("*.cs", "Assets\\WeaverCore\\WeaverCore"))
			{
				Debug.Log("WeaverCOre File = " + file);
			}*/

			weaverCoreBuilder.ReferencePaths.Add(assemblyCSharpBuilder.BuildPath);
			//weaverCoreBuilder.
			yield return weaverCoreBuilder.Build();

			//Debug.Log("BUILD COMPLETE");
		}
	}
}
