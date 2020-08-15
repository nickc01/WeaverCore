using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking.NetworkSystem;
using WeaverBuildTools.Commands;
using WeaverBuildTools.Enums;
using WeaverCore.Editor.Internal;
using WeaverCore.Editor.Utilities;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Systems
{
	public static class LibraryCompiler
	{
		class OnReload : IInit
		{
			void IInit.OnInit()
			{
				if (WeaverReloadTools.DoReloadTools)
				{
					if (WeaverAssetsInfo.InWeaverAssetsProject)
					{
						CopyWeaverAssets("Assets\\WeaverCore", "..\\..");
						BuildPartialWeaverCore();
					}
					else
					{
						CopyWeaverAssets("Assets\\WeaverCore", "Assets\\WeaverCore\\Other Projects~\\Weaver Assets\\Assets\\WeaverCore");
						BuildPartialWeaverCore();

					}
				}
			}
		}



		public static FileInfo DefaultWeaverCoreBuildLocation
		{
			get
			{
				if (WeaverAssetsInfo.InWeaverAssetsProject)
				{
					return new FileInfo("..\\WeaverCore.Game\\WeaverCore Build\\WeaverCore.dll");
				}
				else
				{
					return new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore Build\\WeaverCore.dll");
				}
			}
		}
		public static FileInfo DefaultAssemblyCSharpLocation
		{
			get
			{
				if (WeaverAssetsInfo.InWeaverAssetsProject)
				{
					return new FileInfo("..\\WeaverCore.Game\\WeaverCore Build\\Assembly-CSharp.dll");
				}
				else
				{
					return new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore Build\\Assembly-CSharp.dll");
				}
			}
		}

		/// <summary>
		/// Builds both the HollowKnight assembly and a partial version of WeaverCore that does not contain any embedded resources. This is primarily used with WeaverCore.Game
		/// </summary>
		static void BuildPartialWeaverCore()
		{
			BuildPartialWeaverCore(DefaultWeaverCoreBuildLocation.FullName);
		}

		static void BuildPartialWeaverCore(string buildLocation)
		{
			UnboundCoroutine.Start(BuildPartialWeaverCoreAsync(buildLocation));
		}

		/// <summary>
		/// Builds a full version of WeaverCore that does contain embedded resources.
		/// </summary>
		/// <param name="buildLocation"></param>
		public static void BuildWeaverCore(string buildLocation)
		{
			File.Copy(DefaultWeaverCoreBuildLocation.FullName, buildLocation, true);


			var weaverAssetsLocation = new DirectoryInfo("Assets\\WeaverCore\\WeaverAssets\\Bundles").FullName;

			var windowsBundle = weaverAssetsLocation + "\\weavercore_bundle.bundle.win";
			var linuxBundle = weaverAssetsLocation + "\\weavercore_bundle.bundle.unix";
			var macBundle = weaverAssetsLocation + "\\weavercore_bundle.bundle.mac";
			var weaverGameLocation = new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore.Game\\bin\\WeaverCore.Game.dll");
			var harmonyLocation = new FileInfo("Assets\\WeaverCore\\Libraries\\0Harmony.dll");

			EmbedResourceCMD.EmbedResource(buildLocation, windowsBundle, "weavercore_bundle.bundle.win", compression: CompressionMethod.NoCompression);
			EmbedResourceCMD.EmbedResource(buildLocation, macBundle, "weavercore_bundle.bundle.mac", compression: CompressionMethod.NoCompression);
			EmbedResourceCMD.EmbedResource(buildLocation, linuxBundle, "weavercore_bundle.bundle.unix", compression: CompressionMethod.NoCompression);
			EmbedResourceCMD.EmbedResource(buildLocation, weaverGameLocation.FullName, "WeaverCore.Game", compression: CompressionMethod.NoCompression);
			EmbedResourceCMD.EmbedResource(buildLocation, harmonyLocation.FullName, "0Harmony", compression: CompressionMethod.NoCompression);
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
		static IEnumerator BuildPartialWeaverCoreAsync(string buildLocation)
		{
			yield return BuildHollowKnightASM(DefaultAssemblyCSharpLocation.FullName);

			var weaverCoreBuilder = new Builder();
			weaverCoreBuilder.BuildPath = buildLocation;
			weaverCoreBuilder.Scripts = Builder.GetAllRuntimeInDirectory("*.cs", "Assets\\WeaverCore\\WeaverCore").Where(f => f.Directory.FullName.Contains(""));

			//For some reason, this only works when using forward slashes and not backslashes. Trust me, I even decompiled UnityEditor.dll and checked.
			weaverCoreBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/WeaverCore.dll");
			weaverCoreBuilder.ExcludedReferences.Add("Library/ScriptAssemblies/HollowKnight.dll");

			weaverCoreBuilder.ReferencePaths.Add(DefaultAssemblyCSharpLocation.FullName);


			if (File.Exists(buildLocation))
			{
				File.Delete(buildLocation);
			}
			yield return weaverCoreBuilder.Build();
		}


		static void CopyWeaverAssets(string directoryFrom, string directoryTo)
		{
			var thread = new Thread(() =>
			{
				/*var sourceFolder = new DirectoryInfo("Assets\\WeaverCore");
				var destinationFolder = new DirectoryInfo("Assets\\WeaverCore\\Other Projects~\\Weaver Assets\\Assets\\WeaverCore");
				var destinationAssets = new DirectoryInfo("Assets\\WeaverCore\\Other Projects~\\Weaver Assets\\Assets");*/


				var sourceFolder = new DirectoryInfo(directoryFrom);
				var destinationFolder = new DirectoryInfo(directoryTo);
				var destinationAssets = destinationFolder.Parent;//new DirectoryInfo("Assets\\WeaverCore\\Other Projects~\\Weaver Assets\\Assets");

				if (!destinationAssets.Exists)
				{
					return;
				}

				if (!destinationFolder.Exists)
				{
					destinationFolder.Create();
				}

				//Debug.Log("Source Directory = " + sourceFolder.FullName);
				//Debug.Log("Destination Directory = " + destinationFolder.FullName);
				//Debug.Log("Destination Assets Directory = " + destinationAssets.FullName);

				var sourceFiles = NarrowDown(sourceFolder.GetFiles("*.*", SearchOption.AllDirectories),!WeaverAssetsInfo.InWeaverAssetsProject);//.Where(f => !f.FullName.Contains("Other Projects~") && !f.FullName.Contains(".git") && !f.FullName.Contains("Hidden~\\CopyHashes.txt"));
				var sourceFilesRel = CreateRelativePaths(sourceFiles, sourceFolder.FullName);


				var destFiles = NarrowDown(destinationFolder.GetFiles("*.*", SearchOption.AllDirectories));//.Where(f => !f.FullName.Contains("Other Projects~") && !f.FullName.Contains(".git") && !f.FullName.Contains("Hidden~\\CopyHashes.txt"));
				var destFilesRel = CreateRelativePaths(destFiles, destinationFolder.FullName);

				var differences = GetDifferences(sourceFilesRel, destFilesRel);

				//IF THE COPY WORKED, THIS SHOULD SHOW UP
				//THIS SHOULD IN TURN, BE SEEN IN WEAVERASSETS
				/*foreach (var diff in differences)
				{

					Debug.Log("Diff = " + diff);
				}

				if (WeaverAssetsInfo.InWeaverAssetsProject)
				{
					return;
				}*/

				foreach (var diff in differences)
				{
					File.Delete(destinationFolder.FullName + "\\" + diff);
				}

				var oldHashes = FileHashes.GetHashes();
				var newHashes = new Dictionary<string, string>();

				foreach (var file in sourceFilesRel)
				{
					var fullFilePath = sourceFolder.FullName + "\\" + file;
					var hash = StreamUtilities.GetHash(fullFilePath);

					bool overwriteFile = true;

					newHashes.Add(file, hash);

					if (oldHashes.ContainsKey(file))
					{
						if (oldHashes[file] == hash)
						{
							overwriteFile = false;
						}
					}

					if (overwriteFile)
					{
						var destFullPath = destinationFolder.FullName + "\\" + file;
						if (File.Exists(destFullPath))
						{
							File.Delete(destFullPath);
						}

						var copyDirectory = new FileInfo(destFullPath).Directory;

						if (!copyDirectory.Exists)
						{
							copyDirectory.Create();
						}
						File.Copy(fullFilePath, destFullPath);
					}
				}

				FileHashes.SetHashes(newHashes);
			});

			thread.Start();


		}

		static IEnumerable<FileInfo> NarrowDown(IEnumerable<FileInfo> source, bool excludeOtherProjectDir = true)
		{
			var result = source;
			if (excludeOtherProjectDir)
			{
				result = result.Where(f => !f.FullName.Contains("Other Projects~"));
			}
			result = result.Where(f => !f.FullName.Contains(".git") && !f.FullName.Contains("Hidden~\\CopyHashes.txt"));
			return result;
		}

		static IEnumerable<string> GetDifferences(IEnumerable<string> source, IEnumerable<string> dest)
		{
			/*foreach (var sourceString in source)
			{
				Debug.Log("Source = " + sourceString);
			}

			foreach (var destString in dest)
			{
				Debug.Log("Dest = " + destString);
			}*/
			//return dest.Where(d => !source.Any(s => s == d));
			return dest.Except(source);
		}

		static List<string> CreateRelativePaths(IEnumerable<FileInfo> sourcePaths, string relativeTo)
		{
			List<string> relativePaths = new List<string>();
			foreach (var path in sourcePaths)
			{
				relativePaths.Add(PathUtilities.MakePathRelative(relativeTo, path.FullName));
			}
			return relativePaths;
		}

		[Serializable]
		class FileHashes : ConfigSettings
		{
			[Serializable]
			class HashPair
			{
				public string file;
				public string hash;
			}

			[SerializeField]
			List<HashPair> hashes = new List<HashPair>();

			/// <summary>
			/// Gets the hashes stored in a file. The file path is the key, and the hash is the value
			/// </summary>
			/// <returns></returns>
			public static Dictionary<string, string> GetHashes()
			{
				var hashes = Retrieve<FileHashes>().hashes;
				var hashDictionary = new Dictionary<string, string>();
				foreach (var hash in hashes)
				{
					hashDictionary.Add(hash.file, hash.hash);
				}
				return hashDictionary;
			}

			/// <summary>
			/// Stores the hashes into a file
			/// </summary>
			public static void SetHashes(Dictionary<string, string> hashes)
			{
				//List<HashPair> hashList = new List<HashPair>();
				var instance = Retrieve<FileHashes>();
				instance.hashes.Clear();
				foreach (var pair in hashes)
				{
					instance.hashes.Add(new HashPair() { file = pair.Key, hash = pair.Value });
				}
				instance.SetStoredSettings();
			}
		}
	}
}
