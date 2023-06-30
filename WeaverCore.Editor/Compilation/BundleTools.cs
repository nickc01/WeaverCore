//#define REWRITE_REGISTRIES
#define MULTI_THREADED_EMBEDDING
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WeaverBuildTools.Commands;
using WeaverBuildTools.Enums;
using WeaverCore.Assets;
using WeaverCore.Attributes;
using WeaverCore.Editor.Utilities;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Compilation
{
	/// <summary>
	/// Contains tools needed for building mod asset bundles
	/// </summary>
	public static class BundleTools
	{
		/// <summary>
		/// Used to keep track if this is the first time the user has built a mod
		/// </summary>
		[Serializable]
		class FirstEverBuild
        {
			public bool FirstEver = true;
        }

		/// <summary>
		/// Contains information about a scene in the project
		/// </summary>
		[Serializable]
		public class SceneData
		{
			/// <summary>
			/// The name of the scene
			/// </summary>
			public string Name;

			/// <summary>
			/// The file path of the scene
			/// </summary>
			public string Path;
		}

		/// <summary>
		/// Contains information about a registry
		/// </summary>
		[Serializable]
		public class RegistryInfo
		{
			/// <summary>
			/// The file path of the registry
			/// </summary>
			public string Path;

			/// <summary>
			/// The assembly of the mod the registry is bound to
			/// </summary>
			public string AssemblyName;

			/// <summary>
			/// The type name of the mod the registry is bound to
			/// </summary>
			public string ModTypeName;

			/// <summary>
			/// The asset bundle the registry is a part of
			/// </summary>
			public string AssetBundleName;
		}

		/// <summary>
		/// Used to keep track of excluded asmdef files during the build process
		/// </summary>
		[Serializable]
		public class ExcludedAssembly
		{
			/// <summary>
			/// The assembly name of the asmdef
			/// </summary>
			public string AssemblyName;

			/// <summary>
			/// The original list of included platforms for the asmdef
			/// </summary>
			public List<string> OriginalIncludedPlatforms;

			/// <summary>
			/// The original list of excluded platforms for the asmdef
			/// </summary>
			public List<string> OriginalExcludedPlatforms;
		}

		static BundleBuildData _data = null;

		/// <summary>
		/// Contains all the data regarding the current build process
		/// </summary>
		public static BundleBuildData Data
		{
			get
			{
				if (_data == null)
				{
					if (PersistentData.ContainsData<BundleBuildData>())
					{
						_data = PersistentData.LoadData<BundleBuildData>();
					}
					else
					{
						_data = new BundleBuildData();
					}
				}
				return _data;
			}
			set
			{
				_data = value;
			}
		}

		/// <summary>
		/// A class for containing all the persistent data for a build process
		/// </summary>
		public class BundleBuildData
		{
			/// <summary>
			/// The full path location of the mod dll
			/// </summary>
			public string ModDLL;

			/// <summary>
			/// The full path location of "WeaverCore.dll"
			/// </summary>
			public string WeaverCoreDLL;

			/// <summary>
			/// The name of the mod currently being built
			/// </summary>
			public string ModName;

			/// <summary>
			/// Stores the next method in the build process to be run. This is used to execute the next action in the build process after scripts have been reloaded
			/// </summary>
			public SerializedMethod NextMethod;

			/// <summary>
			/// Stores a list of all the project's assembly information before the build process begins
			/// </summary>
			public List<AssemblyInformation> PreBuildInfo;

			/// <summary>
			/// A list of all the asmdefs being excluded from the build process
			/// </summary>
			public List<ExcludedAssembly> ExcludedAssemblies;

			/// <summary>
			/// Has the asset bundling been successful?
			/// </summary>
			public bool BundlingSuccessful = false;

			/// <summary>
			/// Is WeaverCore only getting built?
			/// </summary>
			public bool WeaverCoreOnly = false;

			/// <summary>
			/// Stores the method to run when the build process is complete
			/// </summary>
			public SerializedMethod OnComplete;

			/// <summary>
			/// Stores a list of all the registry information before the build process begins
			/// </summary>
			public List<RegistryInfo> Registries;

			/// <summary>
			/// Stores a list of all the scenes that have been closed down.
			/// </summary>
			public List<SceneData> ClosedScenes;
		}

		/// <summary>
		/// Builds all the asset bundles in the project and embeds them into the mod assemblies. This method will reload the project several times before completing
		/// </summary>
		/// <param name="modDll">The file path to the mod dll</param>
		/// <param name="weaverCoreDLL">The file path to WeaverCore.dll</param>
		/// <param name="OnComplete">The method to call when the process is done</param>
		public static void BuildAndEmbedAssetBundles(FileInfo modDll, FileInfo weaverCoreDLL, MethodInfo OnComplete)
		{
			Data = new BundleBuildData
			{
				ModDLL = modDll == null ? "" : modDll.FullName,
				ModName = modDll == null ? "" : modDll.Name.Replace(".dll", ""),
				WeaverCoreOnly = modDll == null,
				WeaverCoreDLL = weaverCoreDLL.FullName,
				PreBuildInfo = ScriptFinder.GetProjectScriptInfo(),
				ExcludedAssemblies = new List<ExcludedAssembly>(),
				OnComplete = new SerializedMethod(OnComplete),
				BundlingSuccessful = false
			};
			/**/

			UnboundCoroutine.Start(UnloadScenes(() =>
			{
				PrepareForAssetBundling(new List<string> { "WeaverCore.Editor" }, typeof(BundleTools).GetMethod(nameof(BeginBundleProcess), BindingFlags.Static | BindingFlags.NonPublic));
			}));

			
		}

		/// <summary>
		/// Saves all open scenes and closes them
		/// </summary>
		/// <param name="whenDone">The function to call when done</param>
		static IEnumerator UnloadScenes(Action whenDone)
		{
			EditorSceneManager.SaveOpenScenes();

			yield return null;

			List<Scene> scenes = new List<Scene>();
			Data.ClosedScenes = new List<SceneData>();

			var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
			for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
			{
				var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
				if (scene != activeScene)
				{
					Data.ClosedScenes.Add(new SceneData
					{
						Name = scene.name,
						Path = scene.path
					});
					scenes.Add(scene);
				}
			}

			PersistentData.StoreData(Data);
			PersistentData.SaveData();

			foreach (var scene in scenes)
			{
				var op = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
				yield return new WaitUntil(() => op.isDone);
			}
			Debug.ClearDeveloperConsole();
			yield return null;
			whenDone();
		}

		/// <summary>
		/// Prepares the project for building the asset bundles. This function is used to disable any asmdefs that shouldn't be accounted for when building the asset bundles
		/// </summary>
		/// <param name="ExcludedAssemblies">The asmdefs to exclude</param>
		/// <param name="whenReady">The function to call when done</param>
		static void PrepareForAssetBundling(List<string> ExcludedAssemblies, MethodInfo whenReady)
		{
			Debug.Log("Preparing Assets for Bundling");
#if REWRITE_REGISTRIES
			foreach (var registry in RegistryChecker.LoadAllRegistries())
			{
				registry.ReplaceAssemblyName("Assembly-CSharp", Data.ModName);
				registry.ApplyChanges();
			}
#endif
			Data.Registries = new List<RegistryInfo>();
			var registryIDs = AssetDatabase.FindAssets($"t:{nameof(Registry)}");
			foreach (var id in registryIDs)
			{
				var path = AssetDatabase.GUIDToAssetPath(id);
				var registry = AssetDatabase.LoadAssetAtPath<Registry>(path);
				Data.Registries.Add(new RegistryInfo
				{
					AssemblyName = registry.ModType?.Assembly.GetName().Name ?? "",
					AssetBundleName = GetAssetBundleName(registry),
					ModTypeName = registry.ModName,
					Path = path
				});
			}
			bool assetsChanged = false;
			try
			{
				AssetDatabase.StartAssetEditing();
				foreach (var asm in Data.PreBuildInfo.Where(i => ExcludedAssemblies.Contains(i.AssemblyName)))
				{
					if (asm.Definition.includePlatforms.Count == 1 && asm.Definition.includePlatforms[0] == "Editor")
					{
						continue;
					}
					Data.ExcludedAssemblies.Add(new ExcludedAssembly()
					{
						AssemblyName = asm.AssemblyName,
						OriginalExcludedPlatforms = asm.Definition.excludePlatforms,
						OriginalIncludedPlatforms = asm.Definition.includePlatforms
					});
					asm.Definition.excludePlatforms = new List<string>();
					asm.Definition.includePlatforms = new List<string>
					{
						"Editor"
					};
					asm.Save();
					AssetDatabase.ImportAsset(asm.AssemblyDefinitionPath, ImportAssetOptions.DontDownloadFromCacheServer | ImportAssetOptions.ForceSynchronousImport);
					assetsChanged = true;
				}
				if (assetsChanged)
				{
					Data.NextMethod = new SerializedMethod(whenReady);
				}
				else
				{
					Data.NextMethod = default;
				}
				PersistentData.StoreData(Data);
				PersistentData.SaveData();

				ReflectionUtilities.ExecuteMethodsWithAttribute<BeforeBuildAttribute>();

				if (!assetsChanged && whenReady != null)
				{
					whenReady.Invoke(null, null);
				}
			}
			catch (Exception e)
			{
				Debug.LogError("Exception occured" + e);
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
				UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
			}
		}

		public class MyContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
		{
			protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
			{
				var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
								.Select(p => base.CreateProperty(p, memberSerialization))
							.Union(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
									   .Select(f => base.CreateProperty(f, memberSerialization)))
							.ToList();
				props.ForEach(p => { p.Writable = true; p.Readable = true; });
				return props;
			}
		}

		/// <summary>
		/// Builds all asset bundles in the project
		/// </summary>
		/// <param name="target">The target platform to build the asset bundles against</param>
		/// <param name="group">The target group to build the asset bundles against</param>
		/// <param name="outputDirectory">The output location to put the built bundles in</param>
		static void BuildAssetBundles(BuildTarget target, BuildTargetGroup group, DirectoryInfo outputDirectory)
		{
			var buildInput = ContentBuildInterface.GenerateAssetBundleBuilds().ToList();

			if (Data.WeaverCoreOnly)
			{
				buildInput.RemoveAll(i => i.assetBundleName != "weavercore_bundle");
			}

			foreach (var bundleInput in buildInput)
			{
				Debug.Log("Found Asset Bundle -> " + bundleInput.assetBundleName);
			}

			Assembly scriptBuildAssembly = null;

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (assembly.GetName().Name == "Unity.ScriptableBuildPipeline.Editor")
				{
					scriptBuildAssembly = assembly;
					break;
				}
			}

			if (scriptBuildAssembly == null)
			{
				throw new Exception("The Scriptable Build Pipeline is currently not installed. This is required to build WeaverCore AssetBundles");
			}

			var contentPipelineT = scriptBuildAssembly.GetType("UnityEditor.Build.Pipeline.ContentPipeline");

			var buildAssetBundlesF = contentPipelineT.GetMethods().FirstOrDefault(m => m.GetParameters().Length == 3);

			var bundleBuildParametersT = scriptBuildAssembly.GetType("UnityEditor.Build.Pipeline.BundleBuildParameters");

			var buildParameters = Activator.CreateInstance(bundleBuildParametersT, new object[] { target, group, outputDirectory.FullName });
			bundleBuildParametersT.GetProperty("BundleCompression").SetValue(buildParameters,UnityEngine.BuildCompression.Uncompressed);

			var bundleBuildContentT = scriptBuildAssembly.GetType("UnityEditor.Build.Pipeline.BundleBuildContent");

			var bundleBuildContent = Activator.CreateInstance(bundleBuildContentT, new object[] { buildInput });

			var parameters = new object[] { buildParameters,bundleBuildContent,null };

			var code = buildAssetBundlesF.Invoke(null, parameters);

			var codeType = code.GetType();

			var codeName = Enum.GetName(codeType, code);
			switch (codeName)
			{
				case "Success":
					break;
				case "SuccessCached":
					break;
				case "SuccessNotRun":
					break;
				case "Error":
					throw new BundleException("An error occured when creating an asset bundle");
				case "Exception":
					throw new BundleException("An exception occured when creating an asset bundle");
				case "Canceled":
					throw new BundleException("The asset bundle build was cancelled");
				case "UnsavedChanges":
					throw new BundleException("There are unsaved changes, be sure to save them and try again");
				case "MissingRequiredObjects":
					throw new BundleException("Some required objects are missing");
				default:
					break;
			}
		}

		/// <summary>
		/// Starts building the asset bundles. This is called after <see cref="PrepareForAssetBundling(List{string}, MethodInfo)"/> is called
		/// </summary>
		static void BeginBundleProcess()
		{
			var firstTime = true;
            if (PersistentData.TryGetData(out FirstEverBuild firstBuildData))
            {
				firstTime = firstBuildData.FirstEver;
            }
			UnboundCoroutine.Start(BundleRoutine());
			IEnumerator BundleRoutine()
			{
				yield return new WaitForSeconds(1f);
				try
				{
					Debug.Log("Beginning Bundle Process");
#if !MULTI_THREADED_EMBEDDING
					var builtAssetBundles = new List<BuiltAssetBundle>();
#else
					List<Task> embeddingTasks = new List<Task>();
#endif
					var assemblies = new List<FileInfo>
					{
						new FileInfo(Data.WeaverCoreDLL)
					};

					if (!Data.WeaverCoreOnly)
					{
						assemblies.Add(new FileInfo(Data.ModDLL));
					}

					var temp = Path.GetTempPath();
					var bundleBuilds = new DirectoryInfo(temp + "BundleBuilds\\");

					if (bundleBuilds.Exists)
					{
						bundleBuilds.Delete(true);
					}

					bundleBuilds.Create();

					bool tasksSuccessful = true;

					foreach (var target in BuildScreen.BuildSettings.GetBuildModes())
					{
						if (!PlatformUtilities.IsPlatformSupportLoaded(target))
						{
							continue;
						}
						var targetFolder = bundleBuilds.CreateSubdirectory(target.ToString());

						targetFolder.Create();



						BuildAssetBundles(target, BuildTargetGroup.Standalone, targetFolder);
#if !MULTI_THREADED_EMBEDDING
						foreach (var bundleFile in targetFolder.GetFiles())
						{
							if (bundleFile.Extension == "" && !bundleFile.Name.Contains("BundleBuilds"))
							{
								builtAssetBundles.Add(new BuiltAssetBundle
								{
									File = bundleFile,
									Target = target
								});
							}
						}
#else
						List<BuiltAssetBundle> builtBundles = new List<BuiltAssetBundle>();
						foreach (var bundleFile in targetFolder.GetFiles())
						{
							if (bundleFile.Extension == "" && !bundleFile.Name.Contains("BundleBuilds"))
							{
								Debug.Log("Finished Building Bundle = " + bundleFile.Name);
								builtBundles.Add(new BuiltAssetBundle
								{
									File = bundleFile,
									Target = target
								});
							}
						}
						embeddingTasks.Add(Task.Run(() =>
						{
							try
							{
								if (Data.WeaverCoreOnly)
								{
									EmbedAssetBundles(assemblies, builtBundles.Where(a => a.File.Name.ToLower().Contains("weavercore_bundle")));
								}
								else
								{
									EmbedAssetBundles(assemblies, builtBundles);
								}
							}
							catch (Exception e)
							{
								tasksSuccessful = false;
								Debug.LogError("Error occured when embedding asset bundles");
								Debug.LogException(e);
							}
						}));
#endif
					}

#if !MULTI_THREADED_EMBEDDING
					if (Data.WeaverCoreOnly)
					{
						EmbedAssetBundles(assemblies, builtAssetBundles.Where(a => a.File.Name.ToLower().Contains("weavercore_bundle")));
					}
					else
					{
						EmbedAssetBundles(assemblies, builtAssetBundles);
					}
#else
					Task.WaitAll(embeddingTasks.ToArray());
#endif
					Data.BundlingSuccessful = tasksSuccessful;
					if (tasksSuccessful)
					{
						EmbedWeaverCoreResources();
					}
				}
				catch (Exception e)
				{
                    if (!firstTime)
                    {
						Debug.LogError("Error creating Asset Bundles");
						Debug.LogException(e);
					}
					else
                    {
						DebugUtilities.ClearLog();
                    }
				}
				finally
				{
					if (!Data.BundlingSuccessful && !firstTime)
					{
						Debug.LogError("Error creating Asset Bundles");
					}
					DoneWithAssetBundling(typeof(BundleTools).GetMethod(nameof(CompleteBundlingProcess), BindingFlags.Static | BindingFlags.NonPublic));
				}
			}
		}

		static object embedLock = new object();

		/// <summary>
		/// Embeds the built asset bundles into the mod assemblies
		/// </summary>
		/// <param name="assemblies">The file paths of the mod assemblies to put the asset bundles into</param>
		/// <param name="builtBundles">The asset bundles that have been built</param>
		static void EmbedAssetBundles(List<FileInfo> assemblies, IEnumerable<BuiltAssetBundle> builtBundles)
		{
			Debug.Log("Embedding Asset Bundles");
			var assemblyReplacements = new Dictionary<string, string>
			{
				{"Assembly-CSharp", Data.ModName },
				{"HollowKnight", "Assembly-CSharp" },
				{"HollowKnight.FirstPass", "Assembly-CSharp-firstpass" }
			};

			var bundlePairs = GetBundleToAssemblyPairs(assemblyReplacements);
			foreach (var bundle in builtBundles)
			{
				if (bundlePairs.ContainsKey(bundle.File.Name))
				{
					var asmName = bundlePairs[bundle.File.Name];

					var asmDllName = asmName.Name + ".dll";

					var asmFile = assemblies.FirstOrDefault(a => a.Name == asmDllName);

					if (asmFile != null)
					{
						var processedBundleLocation = PostProcessBundle(bundle, assemblyReplacements);
						lock (embedLock)
						{
							EmbedResourceCMD.EmbedResource(asmFile.FullName, processedBundleLocation, bundle.File.Name + PlatformUtilities.GetBuildTargetExtension(bundle.Target), compression: WeaverBuildTools.Enums.CompressionMethod.NoCompression);
						}

						var sceneBundleName = bundle.File.Name.Replace("_bundle", "_scenes_bundle");
						//Look for Scene Bundle if there is one
						var sceneBundle = new BuiltAssetBundle
						{
							File = new FileInfo(bundle.File.Directory.AddSlash() + sceneBundleName),
							Target = bundle.Target
						};

						if (sceneBundle.File.Exists)
						{
							var processedSceneBundleLocation = PostProcessBundle(sceneBundle, assemblyReplacements);
							lock (embedLock)
							{
								EmbedResourceCMD.EmbedResource(asmFile.FullName, processedSceneBundleLocation, sceneBundle.File.Name + PlatformUtilities.GetBuildTargetExtension(sceneBundle.Target), compression: WeaverBuildTools.Enums.CompressionMethod.NoCompression);
							}
						}
					}
				}
			}
		}


		/// <summary>
		/// Embeds some assemblies and resources into WeaverCore.dll. These are needed for WeaverCore to function when running in Hollow Knight
		/// </summary>
		static void EmbedWeaverCoreResources()
		{
			var weaverGameLocation = new FileInfo(BuildTools.WeaverCoreFolder.AddSlash() + $"Other Projects~{Path.DirectorySeparatorChar}WeaverCore.Game{Path.DirectorySeparatorChar}WeaverCore.Game{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}WeaverCore.Game.dll");
			var harmonyLocation = new FileInfo(BuildTools.WeaverCoreFolder.AddSlash() + $"Libraries{Path.DirectorySeparatorChar}0Harmony.dll");

			EmbedResourceCMD.EmbedResource(Data.WeaverCoreDLL, weaverGameLocation.FullName, "WeaverCore.Game", compression: CompressionMethod.NoCompression);
			EmbedResourceCMD.EmbedResource(Data.WeaverCoreDLL, harmonyLocation.FullName, "0Harmony", compression: CompressionMethod.NoCompression);
		}

		/// <summary>
		/// Pairs up each asset bundle with the assemblies they are to be embedded into
		/// </summary>
		/// <param name="assemblyReplacements">A list of overrides. Used to pair up an asset bundle with a different assembly</param>
		/// <returns></returns>
		static Dictionary<string, AssemblyName> GetBundleToAssemblyPairs(Dictionary<string, string> assemblyReplacements)
		{
			Dictionary<string, AssemblyName> bundleToAssemblyPairs = new Dictionary<string, AssemblyName>();

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (var registry in Data.Registries)
			{
				var originalModName = registry.AssemblyName;

				var assembly = assemblies.FirstOrDefault(a => a.GetName().Name == originalModName);

				if (assembly != null)
				{
					var asmName = assembly.GetName();
					asmName.Name = registry.AssemblyName;
					if (assemblyReplacements.TryGetValue(asmName.Name, out var replacement))
					{
						asmName.Name = replacement;
					}

					bundleToAssemblyPairs.Add(registry.AssetBundleName, asmName);
				}
			}

			return bundleToAssemblyPairs;
		}

		/// <summary>
		/// Gets the asset bundle an object is a part of
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <returns>Returns the name of the asset bundle the object is a part of</returns>
		static string GetAssetBundleName(UnityEngine.Object obj)
		{
			var path = AssetDatabase.GetAssetPath(obj);
			if (path != null && path != "")
			{
				var import = AssetImporter.GetAtPath(path);
				return import.assetBundleName;
			}
			return "";
		}

		/// <summary>
		/// Called when asset bundling is done. This is used for clean up and to undo what <see cref="PrepareForAssetBundling(List{string}, MethodInfo)"/> has done
		/// </summary>
		/// <param name="whenFinished">The function to call when this is done</param>
		static void DoneWithAssetBundling(MethodInfo whenFinished)
		{
#if REWRITE_REGISTRIES
			foreach (var registry in RegistryChecker.LoadAllRegistries())
			{
				registry.ReplaceAssemblyName(Data.ModName, "Assembly-CSharp");
				registry.ApplyChanges();
			}
#endif
			bool assetsChanged = false;
			try
			{
				AssetDatabase.StartAssetEditing();
				if (Data.ExcludedAssemblies != null)
				{
					foreach (var exclusion in Data.ExcludedAssemblies)
					{
						var asmDef = Data.PreBuildInfo.FirstOrDefault(a => a.AssemblyName == exclusion.AssemblyName);
						if (asmDef != null)
						{
							asmDef.Definition.includePlatforms = exclusion.OriginalIncludedPlatforms;
							asmDef.Definition.excludePlatforms = exclusion.OriginalExcludedPlatforms;
							asmDef.Save();
							assetsChanged = true;
							AssetDatabase.ImportAsset(asmDef.AssemblyDefinitionPath, ImportAssetOptions.DontDownloadFromCacheServer | ImportAssetOptions.ForceSynchronousImport);
						}
					}
				}
				if (assetsChanged)
				{
					Data.NextMethod = new SerializedMethod(whenFinished);
				}
				else
				{
					Data.NextMethod = default;
				}
				PersistentData.StoreData(Data);
				PersistentData.SaveData();

				ReflectionUtilities.ExecuteMethodsWithAttribute<AfterBuildAttribute>();

				if (!assetsChanged && whenFinished != null)
				{
					whenFinished.Invoke(null, null);
				}
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
				UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
			}
		}

		/// <summary>
		/// Called when the asset bundling process is fully completed
		/// </summary>
		static void CompleteBundlingProcess()
		{
			var firstTime = true;
			if (PersistentData.TryGetData(out FirstEverBuild firstBuildData))
			{
				firstTime = firstBuildData.FirstEver;
			}
			PersistentData.StoreData(new FirstEverBuild
			{
				FirstEver = false
			});
			PersistentData.SaveData();
			if (!Data.BundlingSuccessful)
			{
                if (firstTime)
                {
					DebugUtilities.ClearLog();
					//Try Building Again, since the first ever build seems to have issues.
					if (BuildScreen.BuildSettings.WeaverCoreOnly)
					{
						BuildTools.BuildWeaverCore();
					}
					else
					{
						BuildTools.BuildMod();
					}
				}
				else
                {
					Debug.LogError("An error occured when creating the asset bundles");
				}
				return;
			}

			Debug.Log("<b>Asset Bundling Complete</b>");
			foreach (var scene in Data.ClosedScenes)
			{
				EditorSceneManager.OpenScene(scene.Path, OpenSceneMode.Additive);
			}
			if (Data.OnComplete.Method != null)
			{
				Data.OnComplete.Method.Invoke(null, null);
			}
		}

		/// <summary>
		/// Used to run AssetTools.Net over a built asset bundle. This is used to make sure that when the asset bundle is loaded in-game, any components on prefabs are pointed to the correct types in the mod assembly
		/// 
		/// This is also used to apply compression on the built asset bundle
		/// </summary>
		/// <param name="bundle">The bundle to post-process</param>
		/// <param name="assemblyReplacements">A list of script assembly names to override</param>
		/// <returns>Returns the path to the post-processed asset bundle</returns>
		static string PostProcessBundle(BuiltAssetBundle bundle, Dictionary<string, string> assemblyReplacements)
		{
			Debug.Log($"Post Processing Bundle -> {bundle.File}");
			var am = new AssetsManager();
			am.LoadClassPackage(BuildTools.WeaverCoreFolder.AddSlash() + $"Libraries{Path.DirectorySeparatorChar}classdata.tpk");

			var bun = am.LoadBundleFile(bundle.File.FullName);

			List<BundleReplacer> bundleReplacers = new List<BundleReplacer>();

			for (int bunIndex = 0; bunIndex < bun.file.bundleInf6.dirInf.GetLength(0); bunIndex++)
			{
				List<AssetsReplacer> assetReplacers = new List<AssetsReplacer>();
				var assetsFileName = bun.file.bundleInf6.dirInf[bunIndex].name; //name of the first entry in the bundle
				if (assetsFileName.EndsWith(".resS") || assetsFileName.EndsWith(".resource"))
				{
					continue;
				}
				//load the first entry in the bundle (hopefully the one we want)
				var assetsFileData = BundleHelper.LoadAssetDataFromBundle(bun.file, bunIndex);

				//I have a new update coming, but in the current release, assetsmanager
				//will only load from file, not from the AssetsFile class. so we just
				//put it into memory and load from that...
				var assetsFileInst = am.LoadAssetsFile(new MemoryStream(assetsFileData), "dummypath" + bunIndex, false);
				var assetsFileTable = assetsFileInst.table;

				//load cldb from classdata.tpk
				am.LoadClassDatabaseFromPackage(assetsFileInst.file.typeTree.unityVersion);


				foreach (var info in assetsFileTable.assetFileInfo)
				{
					//If object is a MonoScript, change the script's "m_AssemblyName" from "Assembly-CSharp" to name of mod assembly
					if (info.curFileType == 0x73)
					{
						//MonoDeserializer.GetMonoBaseField
						var monoScriptInst = am.GetTypeInstance(assetsFileInst.file, info).GetBaseField();
						var m_AssemblyNameValue = monoScriptInst.Get("m_AssemblyName").GetValue();
						foreach (var testAsm in assemblyReplacements)
						{
							var assemblyName = m_AssemblyNameValue.AsString();
							if (assemblyName.Contains(testAsm.Key))
							{
								var newAsmName = assemblyName.Replace(testAsm.Key, testAsm.Value);
								//change m_AssemblyName field
								m_AssemblyNameValue.Set(newAsmName);
								//rewrite the asset and add it to the pending list of changes
								assetReplacers.Add(new AssetsReplacerFromMemory(0, info.index, (int)info.curFileType, 0xffff, monoScriptInst.WriteToByteArray()));
								break;
							}
						}
					}
					//If the object is a MonoBehaviour
#if !REWRITE_REGISTRIES
					else if (info.curFileType == 0x72)
					{
						AssetTypeValueField monoBehaviourInst = null;
						try
						{
                            monoBehaviourInst = am.GetTypeInstance(assetsFileInst, info).GetBaseField();
						}
						catch (Exception e)
						{
							Debug.LogError("An exception occured when reading a MonoBehaviour, skipping");
							foreach (var field in info.GetType().GetFields())
							{
								Debug.LogError($"{field.Name} = {field.GetValue(info)}");
							}
							Debug.LogException(e);
							continue;
						}

						var monoBehaviourName = monoBehaviourInst.Get("m_Name").GetValue().AsString();

						var replacementPair = FontAssetContainer.sourceDestPairs.FirstOrDefault(f => f.Item2 == monoBehaviourName);

                        if (replacementPair != default)
						{
							void ClearArray(string arrayName)
							{
								var arrayField = monoBehaviourInst.Get(arrayName);

								if (!arrayField.IsDummy())
								{
									var arrayPreVal = arrayField.Get("Array");

									var children = arrayPreVal.GetChildrenList();

									Array.Resize(ref children, 0);

									arrayPreVal.SetChildrenList(children);
								}
                            }

							ClearArray("m_glyphInfoList");
							ClearArray("TMP_Glyph_id");
							ClearArray("TMP_Glyph_x");
							ClearArray("TMP_Glyph_y");
							ClearArray("TMP_Glyph_width");
							ClearArray("TMP_Glyph_height");
							ClearArray("TMP_Glyph_xOffset");
							ClearArray("TMP_Glyph_yOffset");
							ClearArray("TMP_Glyph_xAdvance");
							ClearArray("TMP_Glyph_scale");

                            assetReplacers.Add(new AssetsReplacerFromMemory(0, info.index, (int)info.curFileType, AssetHelper.GetScriptIndex(assetsFileInst.file, info), monoBehaviourInst.WriteToByteArray()));
                        }
						else
						{
                            var assemblyField = monoBehaviourInst.Get(str => str.StartsWith("__") && str.Contains("AssemblyName"));

                            if (!assemblyField.IsDummy())
                            {
                                bool modified = false;
                                var modAsmNameVal = assemblyField.GetValue();
                                foreach (var replacement in assemblyReplacements)
                                {
                                    if (modAsmNameVal.AsString() == replacement.Key)
                                    {
                                        modAsmNameVal.Set(replacement.Value);
                                        Debug.Log($"Replacing Assembly Name From {replacement.Key} to {replacement.Value}");
                                        modified = true;
                                        break;
                                    }
                                }

                                var featuresField = monoBehaviourInst.Get("featureAssemblyNames");

                                if (!featuresField.IsDummy())
                                {
                                    var featureAsms = monoBehaviourInst.Get("featureAssemblyNames").Get("Array");
                                    var featureAsmVals = featureAsms.GetValue();
                                    var array = featureAsmVals.AsArray();
                                    for (int i = 0; i < array.size; i++)
                                    {
                                        var asmValue = featureAsms[i].GetValue();
                                        foreach (var replacement in assemblyReplacements)
                                        {
                                            if (asmValue.AsString() == replacement.Key)
                                            {
                                                asmValue.Set(replacement.Value);
                                                Debug.Log($"Replacing Assembly Name From {replacement.Key} to {replacement.Value}");
                                                modified = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (modified)
                                {
                                    assetReplacers.Add(new AssetsReplacerFromMemory(0, info.index, (int)info.curFileType, AssetHelper.GetScriptIndex(assetsFileInst.file, info), monoBehaviourInst.WriteToByteArray()));
                                }
                            }
                        }

						//If this MonoBehaviour has a field called "__modAssemblyName", then it's a Registry object
						//If MonoBehaviour is a registry, replace the "__modAssemblyName" variable from "Assembly-CSharp"
						//__modAssemblyName
					}
					//If the object is a Texture2D
					else if (info.curFileType == 28)
					{
                        try
                        {
                            var texture2DInst = am.GetTypeInstance(assetsFileInst, info).GetBaseField();

							var textureName = texture2DInst.Get("m_Name").GetValue().AsString();

                            if (FontAssetContainer.RemovedTextures.Contains(textureName))
							{
								//WeaverLog.Log("REMOVING Texture = " + textureName);
								assetReplacers.Add(new AssetsRemover(0, info.index, (int)info.curFileType, 0xffff));
							}
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("An exception occured when reading a Texture2D, skipping");
                            foreach (var field in info.GetType().GetFields())
                            {
                                Debug.LogError($"{field.Name} = {field.GetValue(info)}");
                            }
                            Debug.LogException(e);
                            continue;
                        }
                    }
#endif
				}


				//rewrite the assets file back to memory
				byte[] modifiedAssetsFileBytes;
				using (MemoryStream ms = new MemoryStream())
				using (AssetsFileWriter aw = new AssetsFileWriter(ms))
				{
					aw.bigEndian = false;
					assetsFileInst.file.Write(aw, 0, assetReplacers, 0);
					modifiedAssetsFileBytes = ms.ToArray();
				}

				//adding the assets file to the pending list of changes for the bundle
				bundleReplacers.Add(new BundleReplacerFromMemory(assetsFileName, assetsFileName, true, modifiedAssetsFileBytes, modifiedAssetsFileBytes.Length));
			}
			byte[] modifiedBundleBytes;
			using (MemoryStream ms = new MemoryStream())
			using (AssetsFileWriter aw = new AssetsFileWriter(ms))
			{
				bun.file.Write(aw, bundleReplacers);
				modifiedBundleBytes = ms.ToArray();
			}

			using (MemoryStream mbms = new MemoryStream(modifiedBundleBytes))
			using (AssetsFileReader ar = new AssetsFileReader(mbms))
			{
				AssetBundleFile modifiedBundle = new AssetBundleFile();
				modifiedBundle.Read(ar);

				//recompress the bundle and write it (this is optional of course)
				using (FileStream ms = File.OpenWrite(bundle.File.FullName + ".edit"))
				using (AssetsFileWriter aw = new AssetsFileWriter(ms))
				{
					bun.file.Pack(modifiedBundle.reader, aw, BuildScreen.BuildSettings.CompressionType);
				}
			}

			return bundle.File.FullName + ".edit";
		}
	}
}
