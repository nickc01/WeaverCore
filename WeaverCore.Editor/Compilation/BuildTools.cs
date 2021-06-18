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

	public static class BuildTools
	{
		/// <summary>
		/// The default build location for WeaverCore
		/// </summary>
		public static readonly FileInfo WeaverCoreBuildLocation = new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore Build\\WeaverCore.dll");

		///// <summary>
		///// The default build location for WeaverCore.Game
		///// </summary>
		//public static readonly FileInfo WeaverCoreGameBuildLocation = new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore.Game\\bin\\WeaverCore.Game.dll");

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
			task.Result.Success = false;
			yield return BuildWeaverCoreGameRoutine(null, task);
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
			task.Result.Success = false;
			yield return BuildWeaverCoreGameRoutine(null, task);
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
						method.Invoke(null, null);
					}
				}
			}


			if (!ranMethod)
			{
				UnboundCoroutine.Start(BuildPartialWeaverCoreRoutine(WeaverCoreBuildLocation, null));
			}

			UnboundCoroutine.Start(RunPackageCheck());
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
			return BuildXmlProjectRoutine(new FileInfo("Assets\\WeaverCore\\Other Projects~\\WeaverCore.Game\\WeaverCore.Game\\WeaverCore.Game.csproj"), outputPath, task);
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
				XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings
				{
					IgnoreComments = true
				});
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

				reader.ReadToFollowing("ItemGroup");
				while (reader.Read() && reader.Name != "ItemGroup")
				{
					if (reader.Name == "Compile")
					{
						var contents = reader.GetAttribute("Include");
						if (Path.IsPathRooted(contents))
						{
							Scripts.Add(new FileInfo(contents).FullName);
						}
						else
						{
							Scripts.Add(new FileInfo(PathUtilities.AddSlash(xmlProjectFile.Directory.FullName) + contents).FullName);
						}
						if (!reader.IsEmptyElement)
						{
							reader.ReadEndElement();
						}
					}
				}

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
					exclusions.Add(PathUtilities.ConvertToProjectPath(sa.FullName));
					//Debug.Log("Exclusion = " + exclusions[exclusions.Count - 1]);
				}

				var editorDir = new FileInfo(typeof(UnityEditor.EditorWindow).Assembly.Location).Directory;

				var editorUnityEngineDll = new FileInfo(PathUtilities.AddSlash(editorDir.Parent.FullName) + "UnityEngine.dll");

				//Debug.Log("Editor Unity Engine Location = " + editorUnityEngineDll);
				exclusions.Add(editorUnityEngineDll.FullName);

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

		static IEnumerator RunPackageCheck()
		{
			//Debug.Log("Running Package Check");
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
			//yield return new WaitUntil(() => searchRequest.IsCompleted);
			/*foreach (var result in searchRequest.Result)
			{
				Debug.Log("Result = " + result.name + ", Version = " + result.version);
				Debug.Log("Latest Compatible = " + result.versions.latestCompatible);
			}*/

			bool makingChanges = false;
			bool latestVersionInstalled = false;

			foreach (var package in listRequest.Result)
			{
				if (package.name == "com.unity.textmeshpro")
				{
					Debug.Log("Removing Text Mesh Pro package, since WeaverCore provides a version that is compatible with Hollow Knight");
					makingChanges = true;
					PackageClient.Remove(package.name);
					//Break - since we can only do one request at a time
					break;
				}
				else if (package.name == "com.unity.scriptablebuildpipeline")
				{
					//If it isn't the latest compatible version
					if (package.version != buildPipelineLatestVersion)
					{
						Debug.Log($"Updating the Scriptable Build Pipeline from [{package.version}] -> [{buildPipelineLatestVersion}]");
						PackageClient.Remove(package.name);
						makingChanges = true;
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
				PackageClient.Add("com.unity.scriptablebuildpipeline@" + buildPipelineLatestVersion);
			}


			IEnumerator WaitForRequest<T>(Request<T> request) => new WaitUntil(() => request.IsCompleted);
		}
	}
}
