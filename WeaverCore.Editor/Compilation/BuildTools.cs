using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Editor.Settings;
using WeaverCore.Editor.Utilities;
using WeaverCore.Utilities;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

using PackageClient = UnityEditor.PackageManager.Client;

namespace WeaverCore.Editor.Compilation
{
	/// <summary>
	/// Contains the tools needed for building mod assemblies
	/// </summary>
	public static class BuildTools
	{
		static DirectoryInfo weaverCoreFolder;

		/// <summary>
		/// The folder that contains all of WeaverCore's assets
		/// </summary>
		public static DirectoryInfo WeaverCoreFolder
		{
			get
			{
				if (weaverCoreFolder == null)
				{
					var asmDef = PathUtilities.ProjectFolder.GetFiles("WeaverCore.asmdef", SearchOption.AllDirectories);
					if (asmDef.Length > 0)
					{
						weaverCoreFolder = asmDef[0].Directory;
					}
				}
				return weaverCoreFolder;
			}
		}

		/// <summary>
		/// The default build location for WeaverCore
		/// </summary>
		public static FileInfo WeaverCoreBuildLocation = new FileInfo(WeaverCoreFolder.AddSlash() + $"Other Projects~{Path.DirectorySeparatorChar}WeaverCore.Game{Path.DirectorySeparatorChar}WeaverCore Build{Path.DirectorySeparatorChar}WeaverCore.dll");

		/// <summary>
		/// Used to represent an asynchronous assembly build task
		/// </summary>
		class BuildTask<T> : IAsyncBuildTask<T>
		{
			/// <inheritdoc/>
			public T Result { get; set; }

			/// <inheritdoc/>
			public bool Completed { get; set; }

			/// <inheritdoc/>
			object IAsyncBuildTask.Result { get { return Result; } set { Result = (T)value; } }
		}

		/// <summary>
		/// Contains all the parameters needed to build a mod assembly
		/// </summary>
		class BuildParameters
		{
			/// <summary>
			/// The directory the mod is being placed in when built
			/// </summary>
			public DirectoryInfo BuildDirectory;

			/// <summary>
			/// The file name of the built assembly
			/// </summary>
			public string FileName;

			/// <summary>
			/// The full path of where the assembly is going to be located when built
			/// </summary>
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

			/// <summary>
			/// A list of script paths that are included in the build
			/// </summary>
			public List<string> Scripts = new List<string>();

			/// <summary>
			/// A list of assembly reference paths that are to be included in the build process
			/// </summary>
			public List<string> AssemblyReferences = new List<string>();

			/// <summary>
			/// A list of assembly reference paths to be excluded from the build process
			/// </summary>
			public List<string> ExcludedReferences = new List<string>();

			/// <summary>
			/// A list of #defines to include in the build process
			/// </summary>
			public List<string> Defines = new List<string>();

			/// <summary>
			/// The target platform the mod is being built against
			/// </summary>
			public BuildTarget Target = BuildTarget.StandaloneWindows;

			/// <summary>
			/// The target group the mod is being built against
			/// </summary>
			public BuildTargetGroup Group = BuildTargetGroup.Standalone;
		}

		/// <summary>
		/// Contains the output information of a assembly build process
		/// </summary>
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
				AssemblyReferences = hkBuildTask.Result.OutputFiles.Select(f => f.FullName).ToList(),
				Defines = new List<string>
				{
					"GAME_BUILD"
				},
				ExcludedReferences = new List<string>
				{
					"Library/ScriptAssemblies/WeaverCore.dll",
					"Library/ScriptAssemblies/HollowKnight.dll",
					"Library/ScriptAssemblies/HollowKnight.FirstPass.dll",
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

			BuildTask<BuildOutput> firstPassTask = new BuildTask<BuildOutput>();

			var firstPassLocation = new FileInfo(outputPath.Directory.AddSlash() + "Assembly-CSharp-firstpass.dll");

			yield return BuildAssembly(new BuildParameters
			{
				BuildPath = firstPassLocation,
				Scripts = ScriptFinder.FindAssemblyScripts("HollowKnight.FirstPass"),
				Defines = new List<string>
				{
					"GAME_BUILD"
				},
				ExcludedReferences = new List<string>
				{
					"Library/ScriptAssemblies/HollowKnight.dll",
					"Library/ScriptAssemblies/HollowKnight.FirstPass.dll",
					"Library/ScriptAssemblies/JUNK.dll"
				},
			}, BuildPresetType.Game, firstPassTask);

			if (!firstPassTask.Result.Success)
			{
				task.Completed = true;
				task.Result = firstPassTask.Result;
				yield break;
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
					"Library/ScriptAssemblies/HollowKnight.FirstPass.dll",
					"Library/ScriptAssemblies/JUNK.dll"
				},
				AssemblyReferences = new List<string>
				{
					firstPassLocation.FullName
				}
			}, BuildPresetType.Game, task);

			if (!task.Result.Success)
			{
				yield break;
			}
		}

		static IEnumerator BuildAssembly(BuildParameters parameters, BuildPresetType presetType, BuildTask<BuildOutput> task)
		{
			task.Completed = false;
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

			if (builder.Scripts == null || builder.Scripts.Count == 0)
			{
				Debug.LogError("There are no scripts to build");
				task.Result.Success = false;
				task.Completed = true;
				yield break;
			}

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
				Debug.Log("Failed to build WeaverCore");
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
					"Library/ScriptAssemblies/HollowKnight.FirstPass.dll",
					"Library/ScriptAssemblies/JUNK.dll",
					"Library/ScriptAssemblies/WeaverCore.dll",
					"Library/ScriptAssemblies/Assembly-CSharp.dll"
				}
			};

			foreach (var outputFile in weaverCoreTask.Result.OutputFiles)
			{
				parameters.AssemblyReferences.Add(outputFile.FullName);
			}

			yield return BuildAssembly(parameters, BuildPresetType.Game, task);
		}

		/// <summary>
		/// Starts the mod assembly build process
		/// </summary>
		public static void BuildMod()
		{
			BuildMod(new FileInfo(GetModBuildFileLocation()));
		}

		/// <summary>
		/// Stars the mod assembly build process
		/// </summary>
		/// <param name="outputPath">The output location of the mod assembly</param>
		public static void BuildMod(FileInfo outputPath)
		{
			UnboundCoroutine.Start(BuildModRoutine(outputPath, null));
		}

		/// <summary>
		/// Builds the "WeaverCore.dll" assembly
		/// </summary>
		public static void BuildWeaverCore()
		{
			BuildWeaverCore(new FileInfo(GetModBuildFolder() + "WeaverCore.dll"));
		}

		/// <summary>
		/// Builds the "WeaverCore.dll" assembly
		/// </summary>
		/// <param name="outputPath">The output location of the "WeaverCore.dll" assembly</param>
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
			task.Result.Success = false;
			yield return BuildWeaverCoreGameRoutine(null, task);
			if (!task.Result.Success)
			{
				yield break;
			}
			BundleTools.BuildAndEmbedAssetBundles(null, outputPath, typeof(BuildTools).GetMethod(nameof(OnBuildFinish)));
		}

		static IEnumerator BuildModRoutine(FileInfo outputPath, BuildTask<BuildOutput> task)
		{
			if (task == null)
			{
				task = new BuildTask<BuildOutput>();
			}

			var modBuildLocation = new FileInfo(outputPath.Directory.CreateSubdirectory(BuildScreen.BuildSettings.ModName).AddSlash() + BuildScreen.BuildSettings.ModName + ".dll");

			yield return BuildPartialModAsmRoutine(modBuildLocation, task);
			if (!task.Result.Success)
			{
				yield break;
			}
			task.Result.Success = false;
			yield return BuildWeaverCoreGameRoutine(null, task);
			if (!task.Result.Success)
			{
				yield break;
			}
			var weaverCoreOutputLocation = outputPath.Directory.CreateSubdirectory("WeaverCore").AddSlash() + "WeaverCore.dll";
			File.Copy(WeaverCoreBuildLocation.FullName, weaverCoreOutputLocation, true);
			BundleTools.BuildAndEmbedAssetBundles(modBuildLocation, new FileInfo(weaverCoreOutputLocation),typeof(BuildTools).GetMethod(nameof(OnBuildFinish)));
		}

		[OnInit]
		static void Init()
		{
			bool ranMethod = false;

			//Check if a build function is scheduled to run, and run it if there is one
			if (PersistentData.ContainsData<BundleTools.BundleBuildData>())
			{
				var bundleData = BundleTools.Data;
				if (bundleData?.NextMethod.Method != null)
				{
					ranMethod = true;
					var method = bundleData.NextMethod.Method;

					UnboundCoroutine.Start(Delay());

					IEnumerator Delay()
					{
						yield return new WaitForSeconds(0.5f);
						if (EditorApplication.isCompiling)
						{
							yield return new WaitUntil(() => !EditorApplication.isCompiling);
						}
						yield return new WaitForSeconds(0.5f);
						bundleData.NextMethod = default;
						PersistentData.StoreData(bundleData);
						PersistentData.SaveData();
						method.Invoke(null, null);
					}
				}
			}

			//Check dependencies if a build function is not scheduled to be run
			if (!ranMethod)
			{
				DependencyChecker.CheckDependencies();
			}
		}

		/// <summary>
		/// Builds the WeaverCore.Game Assembly
		/// </summary>
		public static void BuildWeaverCoreGameAsm()
		{
			BuildWeaverCoreGameAsm(null);
		}

		/// <summary>
		/// Builds the WeaverCore.Game Assembly
		/// </summary>
		/// <param name="outputPath">The output path of the build. If set to null, it will be placed in the default build location</param>
		public static void BuildWeaverCoreGameAsm(FileInfo outputPath)
		{
			UnboundCoroutine.Start(BuildWeaverCoreGameRoutine(outputPath,null));
		}

		static IEnumerator BuildWeaverCoreGameRoutine(FileInfo outputPath, BuildTask<BuildOutput> task)
		{
			return BuildXmlProjectRoutine(new FileInfo(WeaverCoreFolder.AddSlash() + $"Other Projects~{Path.DirectorySeparatorChar}WeaverCore.Game{Path.DirectorySeparatorChar}WeaverCore.Game{Path.DirectorySeparatorChar}WeaverCore.Game.csproj"), outputPath, task);
		}

		class XmlReference
		{
			public string AssemblyName;
			public FileInfo HintPath;
		}


		static IEnumerator BuildXmlProjectRoutine(FileInfo xmlProjectFile, FileInfo outputPath, BuildTask<BuildOutput> task)
		{
			if (task == null)
			{
				task = new BuildTask<BuildOutput>();
			}
			using (var stream = File.OpenRead(xmlProjectFile.FullName))
			{
				using (var reader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreComments = true }))
				{
					reader.ReadToFollowing("Project");
					reader.ReadToDescendant("PropertyGroup");
					reader.ReadToFollowing("PropertyGroup");
					reader.ReadToDescendant("OutputPath");
					if (outputPath == null)
					{
						var foundOutputPath = reader.ReadElementContentAsString();
						if (Path.IsPathRooted(foundOutputPath))
						{
							outputPath = new FileInfo(PathUtilities.AddSlash(foundOutputPath) + "WeaverCore.Game.dll");
						}
						else
						{
							outputPath = new FileInfo(PathUtilities.AddSlash(xmlProjectFile.Directory.FullName) + PathUtilities.AddSlash(foundOutputPath) + "WeaverCore.Game.dll");
						}

					}
					reader.ReadToFollowing("ItemGroup");
					List<XmlReference> References = new List<XmlReference>();
					List<string> Scripts = new List<string>();
					while (reader.Read())
					{
						if (reader.Name == "Reference")
						{
							var referenceName = reader.GetAttribute("Include");
							FileInfo hintPath = null;
							while (reader.Read() && reader.Name != "Reference")
							{
								if (reader.Name == "HintPath")
								{
									var contents = reader.ReadElementContentAsString();
									if (Path.IsPathRooted(contents))
									{
										hintPath = new FileInfo(contents);
									}
									else
									{
										hintPath = new FileInfo(PathUtilities.AddSlash(xmlProjectFile.Directory.FullName) + contents);
									}
								}
							}
							if (referenceName.Contains("Version=") || referenceName.Contains("Culture=") || referenceName.Contains("PublicKeyToken=") || referenceName.Contains("processorArchitecture="))
							{
								referenceName = new AssemblyName(referenceName).Name;
							}
							References.Add(new XmlReference
							{
								AssemblyName = referenceName,
								HintPath = hintPath
							});
						}
						else if (reader.Name == "ItemGroup")
						{
							break;
						}
					}

					foreach (var scriptFile in xmlProjectFile.Directory.GetFiles("*.cs",SearchOption.AllDirectories))
					{
						Scripts.Add(scriptFile.FullName);
					}

					List<DirectoryInfo> AssemblySearchDirectories = new List<DirectoryInfo>
				{
					new DirectoryInfo(PathUtilities.AddSlash(GameBuildSettings.Settings.HollowKnightLocation) + $"hollow_knight_Data{Path.DirectorySeparatorChar}Managed"),
					new FileInfo(typeof(UnityEditor.EditorWindow).Assembly.Location).Directory
				};

					List<string> AssemblyReferences = new List<string>();



					foreach (var xmlRef in References)
					{
						if (xmlRef.AssemblyName == "System")
						{
							continue;
						}
						if (xmlRef.HintPath != null && xmlRef.HintPath.Exists)
						{
							AssemblyReferences.Add(xmlRef.HintPath.FullName);
						}
						else
						{
							bool found = false;
							foreach (var searchDir in AssemblySearchDirectories)
							{
								var filePath = PathUtilities.AddSlash(searchDir.FullName) + xmlRef.AssemblyName + ".dll";
								if (File.Exists(filePath))
								{
									found = true;
									AssemblyReferences.Add(filePath);
									break;
								}
							}
							if (!found)
							{
								Debug.LogError("Unable to find WeaverCore.Game Reference -> " + xmlRef.AssemblyName);
							}
						}
					}

					var scriptAssemblies = new DirectoryInfo($"Library{Path.DirectorySeparatorChar}ScriptAssemblies").GetFiles("*.dll");

					List<string> exclusions = new List<string>();
					foreach (var sa in scriptAssemblies)
					{
						exclusions.Add(PathUtilities.ConvertToProjectPath(sa.FullName));
					}

					var weaverCoreLibraries = new DirectoryInfo(WeaverCoreFolder.AddSlash() + "Libraries").GetFiles("*.dll");

					foreach (var wl in weaverCoreLibraries)
					{
						exclusions.Add(PathUtilities.ConvertToProjectPath(wl.FullName));
					}

					var editorDir = new FileInfo(typeof(UnityEditor.EditorWindow).Assembly.Location).Directory;

					foreach (var ueFile in editorDir.Parent.GetFiles("UnityEngine.dll", SearchOption.AllDirectories))
					{
						exclusions.Add(ueFile.FullName);
					}

					yield return BuildAssembly(new BuildParameters
					{
						BuildPath = outputPath,
						Scripts = Scripts,
						Defines = new List<string>
					{
						"GAME_BUILD"
					},
						ExcludedReferences = exclusions,
						AssemblyReferences = AssemblyReferences,
					}, BuildPresetType.None, task);

				}
			}

			yield break;
		}

		/// <summary>
		/// Gets the location of where the mod assembly is going to be placed in
		/// </summary>
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

		/// <summary>
		/// Gets the full path of where the mod assembly is going to be placed in
		/// </summary>
		/// <returns></returns>
		public static string GetModBuildFileLocation()
		{
			return GetModBuildFolder() + BuildScreen.BuildSettings.ModName + ".dll";
		}

		/// <summary>
		/// Called when a mod assembly build is completed
		/// </summary>
		public static void OnBuildFinish()
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

			DependencyChecker.CheckDependencies();
		}
	}
}
