using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Editor.Settings;
using WeaverCore.Editor.Utilities;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Compilation
{

	public static class BuildTools
	{
		/// <summary>
		/// The default build location for WeaverCore
		/// </summary>
		public static readonly FileInfo WeaverCoreBuildLocation = new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore Build\\WeaverCore.dll");

		class BuildTask<T> : IAsyncBuildTask<T>
		{
			public T Result { get; set; }

			public bool Completed { get; set; }

			object IAsyncBuildTask.Result { get { return Result; } set { Result = (T)value; } }
		}

		class BuildParameters
		{
			public DirectoryInfo BuildDirectory;
			public string FileName;

			public FileInfo BuildPath
			{
				get
				{
					return new FileInfo(PathUtilities.AddSlash(BuildDirectory.FullName) + FileName);
				}
				set
				{
					BuildDirectory = value.Directory;
					FileName = value.Name;
				}
			}

			public List<string> Scripts = new List<string>();
			public List<string> AssemblyReferences = new List<string>();
			public List<string> ExcludedReferences = new List<string>();
			public List<string> Defines = new List<string>();

			public BuildTarget Target = BuildTarget.StandaloneWindows;
			public BuildTargetGroup Group = BuildTargetGroup.Standalone;
		}

		public class BuildOutput
		{
			public bool Success;
			public List<FileInfo> OutputFiles = new List<FileInfo>();
		}

		/// <summary>
		/// Builds a WeaverCore DLL without any resources embedded into it. This also runs <see cref="BuildHollowKnightAsm(FileInfo)"/> and puts it into the same output directory, since WeaverCore depends on it
		/// </summary>
		/// <returns>The output file</returns>
		public static IAsyncBuildTask<BuildOutput> BuildPartialWeaverCore(FileInfo outputPath)
		{
			var task = new BuildTask<BuildOutput>();
			UnboundCoroutine.Start(BuildPartialWeaverCoreRoutine(outputPath,task));
			return task;
		}

		static IEnumerator BuildPartialWeaverCoreRoutine(FileInfo outputPath, BuildTask<BuildOutput> task)
		{
			if (task == null)
			{
				task = new BuildTask<BuildOutput>();
			}
			task.Result = new BuildOutput();
			var buildDirectory = outputPath.Directory;
			var hkFile = new FileInfo(PathUtilities.AddSlash(buildDirectory.FullName) + "Assembly-CSharp.dll");
			var hkBuildTask = BuildHollowKnightAsm(hkFile);
			yield return new WaitUntil(() => hkBuildTask.Completed);
			task.Result.Success = hkBuildTask.Result.Success;
			task.Result.OutputFiles = hkBuildTask.Result.OutputFiles;
			if (!hkBuildTask.Result.Success)
			{
				task.Completed = true;
				yield break;
			}
			yield return BuildAssembly(new BuildParameters
			{
				BuildPath = outputPath,
				AssemblyReferences = new List<string>
				{
					hkFile.FullName
				},
				Defines = new List<string>
				{
					"GAME_BUILD"
				},
				ExcludedReferences = new List<string>
				{
					"Library/ScriptAssemblies/WeaverCore.dll",
					"Library/ScriptAssemblies/HollowKnight.dll",
					"Library/ScriptAssemblies/WeaverCore.Editor.dll",
					"Library/ScriptAssemblies/WeaverBuildTools.dll",
					"Library/ScriptAssemblies/TMPro.Editor.dll",
					"Library/ScriptAssemblies/0Harmony.dll",
					"Library/ScriptAssemblies/Mono.Cecil.dll",
					"Library/ScriptAssemblies/JUNK.dll"
				},
				Scripts = ScriptFinder.FindAssemblyScripts("WeaverCore")
			}, BuildPresetType.Game,task);
		}

		/// <summary>
		/// Builds a DLL using the scripts from WeaverCore/Hollow Knight
		/// </summary>
		/// <param name="outputPath">The path the file will be placed</param>
		/// <returns></returns>
		public static IAsyncBuildTask<BuildOutput> BuildHollowKnightAsm(FileInfo outputPath)
		{
			var task = new BuildTask<BuildOutput>();
			UnboundCoroutine.Start(BuildHollowKnightAsmRoutine(outputPath, task));
			return task;
		}

		static IEnumerator BuildHollowKnightAsmRoutine(FileInfo outputPath, BuildTask<BuildOutput> task)
		{
			if (task == null)
			{
				task = new BuildTask<BuildOutput>();
			}

			yield return BuildAssembly(new BuildParameters
			{
				BuildPath = outputPath,
				Scripts = ScriptFinder.FindAssemblyScripts("HollowKnight"),
				Defines = new List<string>
				{
					"GAME_BUILD"
				},
				ExcludedReferences = new List<string>
				{
					"Library/ScriptAssemblies/HollowKnight.dll",
					"Library/ScriptAssemblies/JUNK.dll"
				}
			}, BuildPresetType.Game, task);
		}

		static IEnumerator BuildAssembly(BuildParameters parameters, BuildPresetType presetType, BuildTask<BuildOutput> task)
		{
			if (task.Result == null)
			{
				task.Result = new BuildOutput();
			}
			else
			{
				task.Result.Success = false;
			}
			var builder = new AssemblyCompiler();
			builder.BuildDirectory = parameters.BuildDirectory;
			builder.FileName = parameters.FileName;
			builder.Scripts = parameters.Scripts;
			builder.Target = parameters.Target;
			builder.TargetGroup = parameters.Group;
			builder.References = parameters.AssemblyReferences;
			builder.ExcludedReferences = parameters.ExcludedReferences;
			builder.Defines = parameters.Defines;

			if (presetType == BuildPresetType.Game || presetType == BuildPresetType.Editor)
			{
				builder.AddUnityReferences();
			}
			if (presetType == BuildPresetType.Game)
			{
				builder.RemoveEditorReferences();
			}

			if (!parameters.BuildPath.Directory.Exists)
			{
				parameters.BuildPath.Directory.Create();
			}
			if (parameters.BuildPath.Exists)
			{
				parameters.BuildPath.Delete();
			}

			AssemblyCompiler.OutputDetails output = new AssemblyCompiler.OutputDetails();
			yield return builder.Build(output);
			if (output.Success)
			{
				task.Result.OutputFiles.Add(parameters.BuildPath);
			}
			task.Result.Success = output.Success;
			task.Completed = true;
		}

		static IEnumerator BuildPartialModAsmRoutine(FileInfo outputPath, BuildTask<BuildOutput> task)
		{
			if (task == null)
			{
				task = new BuildTask<BuildOutput>();
			}

			var weaverCoreTask = BuildPartialWeaverCore(WeaverCoreBuildLocation);

			yield return new WaitUntil(() => weaverCoreTask.Completed);

			if (!weaverCoreTask.Result.Success)
			{
				task.Completed = true;
				task.Result = new BuildOutput
				{
					Success = false
				};
				yield break;
			}

			var parameters = new BuildParameters
			{
				BuildPath = outputPath,
				Scripts = ScriptFinder.FindAssemblyScripts("Assembly-CSharp"),
				Defines = new List<string>
				{
					"GAME_BUILD"
				},
				ExcludedReferences = new List<string>
				{
					"Library/ScriptAssemblies/HollowKnight.dll",
					"Library/ScriptAssemblies/JUNK.dll",
					"Library/ScriptAssemblies/WeaverCore.dll",
					"Library/ScriptAssemblies/Assembly-CSharp.dll"
				}
			};

			foreach (var outputFile in weaverCoreTask.Result.OutputFiles)
			{
				Debug.Log("New Reference = " + outputFile.FullName);
				parameters.AssemblyReferences.Add(outputFile.FullName);
			}

			yield return BuildAssembly(parameters, BuildPresetType.Game, task);
		}

		public static void BuildMod()
		{
			BuildMod(new FileInfo(GetModBuildFileLocation()));
		}

		public static void BuildMod(FileInfo outputPath)
		{
			UnboundCoroutine.Start(BuildModRoutine(outputPath, null));
		}

		public static void BuildWeaverCore()
		{
			BuildWeaverCore(new FileInfo(GetModBuildFolder() + "WeaverCore.dll"));
		}

		public static void BuildWeaverCore(FileInfo outputPath)
		{
			UnboundCoroutine.Start(BuildWeaverCoreRoutine(outputPath, null));
		}

		static IEnumerator BuildWeaverCoreRoutine(FileInfo outputPath, BuildTask<BuildOutput> task)
		{
			if (task == null)
			{
				task = new BuildTask<BuildOutput>();
			}
			yield return BuildPartialWeaverCoreRoutine(outputPath, task);
			if (!task.Result.Success)
			{
				yield break;
			}
			BundleTools.BuildAndEmbedAssetBundles(null, outputPath, typeof(BuildTools).GetMethod(nameof(StartHollowKnight)));
		}

		static IEnumerator BuildModRoutine(FileInfo outputPath, BuildTask<BuildOutput> task)
		{
			if (task == null)
			{
				task = new BuildTask<BuildOutput>();
			}
			yield return BuildPartialModAsmRoutine(outputPath, task);
			if (!task.Result.Success)
			{
				yield break;
			}
			var weaverCoreOutputLocation = PathUtilities.AddSlash(outputPath.Directory.FullName) + "WeaverCore.dll";
			File.Copy(WeaverCoreBuildLocation.FullName, weaverCoreOutputLocation, true);
			BundleTools.BuildAndEmbedAssetBundles(outputPath, new FileInfo(weaverCoreOutputLocation),typeof(BuildTools).GetMethod(nameof(StartHollowKnight)));
		}


		[OnInit]
		static void Init()
		{
			bool ranMethod = false;

			if (PersistentData.ContainsData<BundleTools.BundleBuildData>())
			{
				var bundleData = BundleTools.Data;
				if (bundleData?.NextMethod.Method != null)
				{
					ranMethod = true;
					var method = bundleData.NextMethod.Method;
					bundleData.NextMethod = default;

					PersistentData.StoreData(bundleData);
					PersistentData.SaveData();

					UnboundCoroutine.Start(Delay());

					IEnumerator Delay()
					{
						yield return new WaitForSeconds(0.5f);
						method.Invoke(null, null);
					}
				}
			}


			if (!ranMethod)
			{
				UnboundCoroutine.Start(BuildPartialWeaverCoreRoutine(WeaverCoreBuildLocation, null));
			}
		}

		public static string GetModBuildFolder()
		{
			string compileLocation = null;
			if (compileLocation == null)
			{
				if (!string.IsNullOrEmpty(BuildScreen.BuildSettings?.BuildLocation))
				{
					compileLocation = BuildScreen.BuildSettings.BuildLocation;
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
			return PathUtilities.AddSlash(compileLocation);
		}

		public static string GetModBuildFileLocation()
		{
			return GetModBuildFolder() + BuildScreen.BuildSettings.ModName + ".dll";
		}

		public static void StartHollowKnight()
		{
			if (BuildScreen.BuildSettings.StartGame)
			{
				Debug.Log("<b>Starting Game</b>");
				var hkEXE = new FileInfo(GameBuildSettings.Settings.HollowKnightLocation + "\\hollow_knight.exe");


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
