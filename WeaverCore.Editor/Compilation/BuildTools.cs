﻿using System;
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
	/*class WeaverCoreReady
	{
		public bool Ready = false;
	}*/


	public static class BuildTools
	{
		static DirectoryInfo weaverCoreFolder;
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
		public static FileInfo WeaverCoreBuildLocation = new FileInfo(WeaverCoreFolder.AddSlash() + "Other Projects~\\WeaverCore.Game\\WeaverCore Build\\WeaverCore.dll");

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
				//Debug.Log("Failed to build HollowKnight.dll");
				task.Completed = true;
				yield break;
			}
			yield return BuildAssembly(new BuildParameters
			{
				BuildPath = outputPath,
				AssemblyReferences = hkBuildTask.Result.OutputFiles.Select(f => f.FullName).ToList(),
				/*AssemblyReferences = new List<string>
				{
					hkFile.FullName,
					hkFile.AddSlash()
				},*/
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
				//Debug.Log("Failed to build HollowKnight.FirstPass.dll");
				task.Completed = true;
				task.Result = firstPassTask.Result;
				yield break;
			}

			//Debug.Log("FIRST PASS BUILT!");

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

			//Debug.Log("SECOND PASS BUILT!");

			if (!task.Result.Success)
			{
				//Debug.Log("Failed to build HK.dll");
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
			//Debug.Log("OUTPUT = " + output.Success);
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
				//Debug.Log("New Reference = " + outputFile.FullName);
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
				//Debug.Log("Failed to build Assembly-CSharp.dll");
				yield break;
			}
			//Debug.Log("Done Building Assembly-CSharp.dll");
			task.Result.Success = false;
			//Debug.Log("Building WeaverCore.Game.dll");
			yield return BuildWeaverCoreGameRoutine(null, task);
			if (!task.Result.Success)
			{
				//Debug.Log("Failed to build WeaverCore.Game.dll");
				yield break;
			}
			var weaverCoreOutputLocation = outputPath.Directory.CreateSubdirectory("WeaverCore").AddSlash() + "WeaverCore.dll";
			File.Copy(WeaverCoreBuildLocation.FullName, weaverCoreOutputLocation, true);
			BundleTools.BuildAndEmbedAssetBundles(modBuildLocation, new FileInfo(weaverCoreOutputLocation),typeof(BuildTools).GetMethod(nameof(OnBuildFinish)));
		}

		[OnInit]
		static void Init()
		{
			//BuildWeaverCoreGameAsm();
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
						if (EditorApplication.isCompiling)
						{
							//Debug.Log("A_ Waiting for Compilation to finish");
							yield return new WaitUntil(() => !EditorApplication.isCompiling);
						}
						//Debug.Log("A_Compilation Done!");
						method.Invoke(null, null);
					}
				}
			}


			if (!ranMethod)
			{
				DependencyChecker.CheckDependencies();
				/*IEnumerator DefaultRoutine()
				{
					if (EditorApplication.isCompiling)
					{
						//Debug.Log("B_ Waiting for Compilation to finish");
						yield return new WaitUntil(() => !EditorApplication.isCompiling);
					}

					if (!PersistentData.TryGetData<WeaverCoreReady>(out var ready) || !ready.Ready)
					{
						DebugUtilities.ClearLog();
					}

					//Debug.Log("B_Compilation Done!");
					//yield return BuildPartialWeaverCoreRoutine(WeaverCoreBuildLocation, null);
					//yield return RunEnvironmentCheck();
				}

				if (!EditorApplication.isPlaying)
				{
					UnboundCoroutine.Start(DefaultRoutine());
				}*/
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
			return BuildXmlProjectRoutine(new FileInfo(WeaverCoreFolder.AddSlash() + "Other Projects~\\WeaverCore.Game\\WeaverCore.Game\\WeaverCore.Game.csproj"), outputPath, task);
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
					//reader.NodeType == XmlNodeType.Prope
					//reader.ReadEndElement();
					//reader.ReadToFollowing("PropertyGroup");
					//reader.ReadToFollowing("OutputPath");

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
					//Debug.Log("Output Path = " + foundOutputPath);
					//reader.ReadEndElement();
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
									//reader.ReadEndElement();
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

					/*reader.ReadToFollowing("ItemGroup");
					while (reader.Read() && reader.Name != "ItemGroup")
					{
						if (reader.Name == "Compile")
						{
							var contents = reader.GetAttribute("Include");
							if (Path.IsPathRooted(contents))
							{
								//Scripts.Add(new FileInfo(contents).FullName);
							}
							else
							{
								//Scripts.Add(new FileInfo(PathUtilities.AddSlash(xmlProjectFile.Directory.FullName) + contents).FullName);
							}
							if (!reader.IsEmptyElement)
							{
								reader.ReadEndElement();
							}
						}
					}*/

					/*Debug.Log("Results!");
					foreach (var r in References)
					{
						Debug.Log($"Assembly Name = {r.AssemblyName}, Hint Path = {r.HintPath?.FullName}");
					}

					foreach (var s in Scripts)
					{
						Debug.Log("Script = " + s);
					}

					Debug.Log("Output Path = " + outputPath);*/





					/*while (reader.Read())
					{
						Debug.Log("Reader Name = " + reader.Name);
						Debug.Log("Reader Local Name = " + reader.LocalName);
						//reader.IsStartElement()
					}*/

					List<DirectoryInfo> AssemblySearchDirectories = new List<DirectoryInfo>
				{
					new DirectoryInfo(PathUtilities.AddSlash(GameBuildSettings.Settings.HollowKnightLocation) + "hollow_knight_Data\\Managed"),
					new FileInfo(typeof(UnityEditor.EditorWindow).Assembly.Location).Directory
				};

					/*foreach (var searchDir in AssemblySearchDirectories)
					{
						Debug.Log("Search Directory = " + searchDir.FullName);
					}*/

					List<string> AssemblyReferences = new List<string>();



					foreach (var xmlRef in References)
					{
						/*if (xmlRef.AssemblyName.Contains("UnityEngine"))
						{
							continue;
						}*/
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
								//Debug.Log("File Path = " + filePath);
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

					var scriptAssemblies = new DirectoryInfo("Library\\ScriptAssemblies").GetFiles("*.dll");

					List<string> exclusions = new List<string>();
					foreach (var sa in scriptAssemblies)
					{
						//Debug.Log("PROJECT FOLDER = " + PathUtilities.ProjectFolder);
						//Debug.Log("FULL PATH = " + sa.FullName);
						//Debug.Log("EXCLUDING ASSEMBLY = " + PathUtilities.ConvertToProjectPath(sa.FullName));
						exclusions.Add(PathUtilities.ConvertToProjectPath(sa.FullName));
						//Debug.Log("Exclusion = " + exclusions[exclusions.Count - 1]);
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

					//var editorUnityEngineDll = new FileInfo(PathUtilities.AddSlash(editorDir.Parent.FullName) + "UnityEngine.dll");

					//Debug.Log("Editor Unity Engine Location = " + editorUnityEngineDll);
					//exclusions.Add(editorUnityEngineDll.FullName);

					//Debug.Log("Editor Location = " + );
					//Debug.Log("System Location = " + typeof(System.Array).Assembly.Location);

					//exclusions.Add("Library\\ScriptAssemblies\\HollowKnight.dll");

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
			//UnboundCoroutine.Start(RunEnvironmentCheck());
		}

		/*static IEnumerator RunEnvironmentCheck()
		{
			//INSTALL THE NECESSARY PACKAGES
			var listRequest = PackageClient.List();
			var searchRequest = PackageClient.Search("com.unity.scriptablebuildpipeline");
			yield return WaitForRequest(listRequest);
			if (listRequest.Status == StatusCode.Failure)
			{
				Debug.LogError($"Failed to get a package listing with error code [{listRequest.Error.errorCode}], {listRequest.Error.message}");
			}
			yield return WaitForRequest(searchRequest);
			if (searchRequest.Status == StatusCode.Failure)
			{
				Debug.LogError($"Failed to do a package search with the error code [{searchRequest.Error.errorCode}], {searchRequest.Error.message}");
			}

			var buildPipelineLatestVersion = searchRequest.Result[0].versions.latestCompatible;

			bool makingChanges = false;
			bool latestVersionInstalled = false;

			foreach (var package in listRequest.Result)
			{
				if (package.name == "com.unity.textmeshpro")
				{
					DebugUtilities.ClearLog();
					Debug.Log("Removing Text Mesh Pro package, since WeaverCore provides a version that is compatible with Hollow Knight");
					makingChanges = true;
					PackageClient.Remove(package.name);
					//DebugUtilities.ClearLog();
					//Break - since we can only do one request at a time
					break;
				}
				else if (package.name == "com.unity.scriptablebuildpipeline")
				{
					//If it isn't the latest compatible version
					if (package.version != buildPipelineLatestVersion)
					{
						DebugUtilities.ClearLog();
						Debug.Log($"Updating the Scriptable Build Pipeline from [{package.version}] -> [{buildPipelineLatestVersion}]");
						PackageClient.Remove(package.name);
						makingChanges = true;
						//DebugUtilities.ClearLog();
						//Break - since we can only do one request at a time
						break;
					}
					else
					{
						latestVersionInstalled = true;
					}

				}
			}

			if (!makingChanges && !latestVersionInstalled)
			{
				DebugUtilities.ClearLog();
				PackageClient.Add("com.unity.scriptablebuildpipeline@" + buildPipelineLatestVersion);
				makingChanges = true;
			}

			if (!makingChanges)
			{
				if (PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone) != ApiCompatibilityLevel.NET_4_6)
				{
					DebugUtilities.ClearLog();
					Debug.Log("Updating Project API Level from .Net Standard 2.0 to .Net 4.6");
					PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_4_6);
					makingChanges = true;
				}
			}

			if (!makingChanges)
			{
				var projectInfo = ScriptFinder.GetProjectScriptInfo();

				var asm = projectInfo.FirstOrDefault(p => p.AssemblyName.Contains("WeaverCore.Editor"));

				if (asm == null)
				{
					throw new Exception("Unable to find assembly \"WeaverCore.Editor\". Your WeaverCore files may not be in a valid state");
				}

				if (asm.Definition.includePlatforms.Count > 0)
				{
					makingChanges = true;
					asm.Definition.excludePlatforms = new List<string>();
					asm.Definition.includePlatforms = new List<string>();
					//Debug.Log("Asm Definition Path = " + asm.AssemblyDefinitionPath);
					//Debug.Log("Importing Asset = " + asm.AssemblyDefinitionPath);
					asm.Save();

					DebugUtilities.ClearLog();
					Debug.Log("Updating WeaverCore.Editor State");

					AssetDatabase.ImportAsset(asm.AssemblyDefinitionPath,ImportAssetOptions.DontDownloadFromCacheServer);
					AssetDatabase.Refresh();
				}
			}

			if (!makingChanges)
			{
				if (!PersistentData.TryGetData<WeaverCoreReady>(out var ready) || !ready.Ready)
				{
					DebugUtilities.ClearLog();
					Debug.Log("WeaverCore is Ready for Use!");
					PersistentData.StoreData(new WeaverCoreReady
					{
						Ready = true
					});

					PersistentData.SaveData();
				}
			}

			IEnumerator WaitForRequest<T>(Request<T> request) => new WaitUntil(() => request.IsCompleted);
		}*/
	}
}
