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
//using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.SceneManagement;
using WeaverBuildTools.Commands;
using WeaverBuildTools.Enums;
using WeaverCore.Attributes;
using WeaverCore.Editor.Internal;
using WeaverCore.Editor.Utilities;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Compilation
{
	public static class BundleTools
	{
		[Serializable]
		public class SceneData
		{
			public string Name;
			public string Path;
		}

		[Serializable]
		public class RegistryInfo
		{
			public string Path;
			public string AssemblyName;
			public string ModTypeName;
			public string AssetBundleName;
		}

		[Serializable]
		public class ExcludedAssembly
		{
			public string AssemblyName;
			public List<string> OriginalIncludedPlatforms;
			public List<string> OriginalExcludedPlatforms;
		}

		static BundleBuildData _data = null;

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

		public class BundleBuildData
		{
			public string ModDLL;
			public string WeaverCoreDLL;

			public string ModName;

			public SerializedMethod NextMethod;

			public List<AssemblyInformation> PreBuildInfo;

			//public List<string> ExcludedAssemblies;
			public List<ExcludedAssembly> ExcludedAssemblies;

			public bool BundlingSuccessful = false;
			public bool WeaverCoreOnly = false;

			public SerializedMethod OnComplete;

			public List<RegistryInfo> Registries;

			public List<SceneData> ClosedScenes;
		}

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

		static bool IsFileLocked(FileInfo file)
		{
			try
			{
				using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
				{
					stream.Close();
				}
			}
			catch (IOException)
			{
				//the file is unavailable because it is:
				//still being written to
				//or being processed by another thread
				//or does not exist (has already been processed)
				return true;
			}

			//file is not locked
			return false;
		}

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

		static void PrepareForAssetBundling(List<string> ExcludedAssemblies, MethodInfo whenReady)
		{
			//AssetDatabase.DisallowAutoRefresh();
			Debug.Log("Preparing Assets for Bundling");

			

			//UnboundCoroutine.Start(Delay());
			//IEnumerator Delay()
			//{
			/*yield return new WaitForSeconds(0.5f);
			foreach (var registry in RegistryChecker.LoadAllRegistries())
			{
				registry.ReplaceAssemblyName("Assembly-CSharp", Data.ModName);
				registry.ApplyChanges();
			}*/
			//Debug.Log("A_Editor File Locked = " + IsFileLocked(new FileInfo("Library\\ScriptAssemblies\\WeaverCore.Editor.dll")));
			/*var buildCache = new DirectoryInfo("Library\\BuildCache");
			if (buildCache.Exists)
			{
				buildCache.Delete(true);
			}*/
			//yield return new WaitForSeconds(0.5f);
			//Debug.Log("B_Editor File Locked = " + IsFileLocked(new FileInfo("Library\\ScriptAssemblies\\WeaverCore.Editor.dll")));
			//yield return new WaitUntil(() => !EditorApplication.isCompiling);
			//AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
			//yield return new WaitForSeconds(0.5f);
			//Debug.Log("C_Editor File Locked = " + IsFileLocked(new FileInfo("Library\\ScriptAssemblies\\WeaverCore.Editor.dll")));
			//yield return new WaitUntil(() => !EditorApplication.isCompiling);
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
			//yield return new WaitForSeconds(0.5f);
			//Debug.Log("D_Editor File Locked = " + IsFileLocked(new FileInfo("Library\\ScriptAssemblies\\WeaverCore.Editor.dll")));
			//yield return new WaitUntil(() => !EditorApplication.isCompiling);
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
					//Debug.Log("Asm Definition Path = " + asm.AssemblyDefinitionPath);
					//Debug.Log("Importing Asset = " + asm.AssemblyDefinitionPath);
					asm.Save();
					AssetDatabase.ImportAsset(asm.AssemblyDefinitionPath, ImportAssetOptions.DontDownloadFromCacheServer);
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
				//if (assetsChanged)
				//{
				//AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
				//}
			}
			//}
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

		static string RemoveIllegalCharacters(string illegal)
		{
			string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

			foreach (char c in invalid)
			{
				illegal = illegal.Replace(c.ToString(), "");
			}
			return illegal;
		}

		public static void Test()
		{
			Debug.Log("TESTING TESTING TESTING");
			var interfaceType = typeof(AnimationMode).Assembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");
			var editorCompInstance = interfaceType.GetProperty("Instance").GetValue(null, null);

			var scriptAssembliesDict = (IDictionary)editorCompInstance.GetType().GetField("filePathToCustomScriptAssemblies", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(editorCompInstance);

			var projectInfo = ScriptFinder.GetProjectScriptInfo();

			var editorWeaverCoreASM = projectInfo.FirstOrDefault(a => a.AssemblyName.Contains("WeaverCore.Editor"));

			var dirtyChangedAssemblyDefinitionFunc = editorCompInstance.GetType().GetMethod("DirtyChangedAssemblyDefinition");

			Debug.Log("WeaverCore.Editor Path = " + editorWeaverCoreASM.AssemblyDefinitionPath);

			//dirtyChangedAssemblyDefinitionFunc.Invoke(editorCompInstance, new object[] { editorWeaverCoreASM.AssemblyDefinitionPath });

			Debug.Log("ENTIRE OBJECT");

			var settings = new JsonSerializerSettings() 
			{ 
				ContractResolver = new MyContractResolver(),
				Formatting = Formatting.Indented,
				ReferenceLoopHandling = ReferenceLoopHandling.Serialize
			};

			foreach (var field in editorCompInstance.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (field.Name != "ILPostProcessing")
				{
					Debug.Log($"{field.Name}");
					//Debug.Log();
					File.WriteAllText($"C:/ScratchDump/{RemoveIllegalCharacters(field.Name)}.json", JsonConvert.SerializeObject(field.GetValue(editorCompInstance), settings));
				}
			}

			foreach (var prop in editorCompInstance.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				Debug.Log($"{prop.Name} = ");
				//Debug.Log(JsonConvert.SerializeObject(prop.GetValue(editorCompInstance), settings));
				File.WriteAllText($"C:/ScratchDump/{RemoveIllegalCharacters(prop.Name)}.json", JsonConvert.SerializeObject(prop.GetValue(editorCompInstance), settings));
			}

			//Debug.Log(JsonConvert.SerializeObject(editorCompInstance, settings));

			foreach (var item in scriptAssembliesDict)
			{
				Debug.Log("ASM = " + JsonConvert.SerializeObject(item, Formatting.Indented));
			}

			/*foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				Debug.Log("ASM = " + asm.Location);
			}*/
		}

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
			//Debug.Log("Code Name = " + codeName);
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



			/*var code = ContentPipeline.BuildAssetBundles(new BundleBuildParameters(target, BuildTargetGroup.Standalone, targetFolder.FullName)
			{
				BundleCompression = UnityEngine.BuildCompression.LZ4,
			}, new BundleBuildContent(buildInput), out var results);*/

			/*switch (code)
			{
				case ReturnCode.Success:
					break;
				case ReturnCode.SuccessCached:
					break;
				case ReturnCode.SuccessNotRun:
					break;
				case ReturnCode.Error:
					throw new BundleException("An error occured when creating an asset bundle");
				case ReturnCode.Exception:
					throw new BundleException("An exception occured when creating an asset bundle");
				case ReturnCode.Canceled:
					throw new BundleException("The asset bundle build was cancelled");
				case ReturnCode.UnsavedChanges:
					throw new BundleException("There are unsaved changes, be sure to save them and try again");
				case ReturnCode.MissingRequiredObjects:
					throw new BundleException("Some required objects are missing");
				default:
					break;
			}*/
		}

		static void BeginBundleProcess()
		{
			UnboundCoroutine.Start(BundleRoutine());
			IEnumerator BundleRoutine()
			{
				yield return new WaitForSeconds(1f);
				try
				{
					//Debug.Log("E_Editor File Locked = " + IsFileLocked(new FileInfo("Library\\ScriptAssemblies\\WeaverCore.Editor.dll")));
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

					//BuildSettings settings = new BuildSettings();
					//settings.GetStoredSettings();

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
							//Debug.Log("RUNNING TASK!");
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
					//Debug.Log("WAITING FOR TASKS");
					Task.WaitAll(embeddingTasks.ToArray());
					//Debug.Log("FINISHED WITH TASKS");
#endif
					Data.BundlingSuccessful = tasksSuccessful;
					if (tasksSuccessful)
					{
						EmbedWeaverCoreResources();
					}
				}
				catch (Exception e)
				{
					Debug.LogError("Error creating Asset Bundles");
					Debug.LogException(e);
				}
				finally
				{
					if (!Data.BundlingSuccessful)
					{
						Debug.LogError("Error creating Asset Bundles");
					}
					//Debug.Log("POST_4_Editor File Locked = " + IsFileLocked(new FileInfo("Library\\ScriptAssemblies\\WeaverCore.Editor.dll")));
					//UnboundCoroutine.Start(Delay());
					//IEnumerator Delay()
					//{
						//Debug.Log("POST_5_Editor File Locked = " + IsFileLocked(new FileInfo("Library\\ScriptAssemblies\\WeaverCore.Editor.dll")));
						//yield return new WaitForSeconds(0.5f);
						//Debug.Log("F_Editor File Locked = " + IsFileLocked(new FileInfo("Library\\ScriptAssemblies\\WeaverCore.Editor.dll")));
						DoneWithAssetBundling(typeof(BundleTools).GetMethod(nameof(CompleteBundlingProcess), BindingFlags.Static | BindingFlags.NonPublic));
					//}
				}
			}
			
			//Debug.Log("Beginning Bundle Process!!!");
		}

		/*static void EmbedAssetBundle(List<FileInfo> assemblies, IEnumerable<BuiltAssetBundle> builtBundles, Dictionary<string,AssemblyName> bundleToAssemblyPairs)
		{

		}*/

		static object embedLock = new object();

		static void EmbedAssetBundles(List<FileInfo> assemblies, IEnumerable<BuiltAssetBundle> builtBundles)
		{
			Debug.Log("Embedding Asset Bundles");
			var assemblyReplacements = new Dictionary<string, string>
			{
				{"Assembly-CSharp", Data.ModName },
				{"HollowKnight", "Assembly-CSharp" },
				{"HollowKnight.FirstPass", "Assembly-CSharp-firstpass" }
			};
			/*foreach (var assembly in assemblies)
			{
				Debug.Log("Built Assembly = " + assembly.FullName);
			}
			foreach (var bundle in builtBundles)
			{
				Debug.Log("Built Bundle = " + bundle.File.FullName);
			}*/
			var bundlePairs = GetBundleToAssemblyPairs(assemblyReplacements);
			/*foreach (var pair in bundlePairs)
			{
				Debug.Log($"Bundle Pair = {pair.Key} - {pair.Value.Name}");
			}*/
			/*if (builtBundles.Any())
			{
				Debug.Log("THERE ARE BUNDLES");
			}
			if (bundlePairs.Any())
			{
				Debug.Log("THERE ARE PAIRS");
			}
			foreach (var pair in bundlePairs)
			{
				Debug.Log($"PAIR = {pair.Key}, {pair.Value}");
			}*/
			foreach (var bundle in builtBundles)
			{
				//Debug.Log("BUNDLE = " + bundle.File.Name);
				if (bundlePairs.ContainsKey(bundle.File.Name))
				{
					var asmName = bundlePairs[bundle.File.Name];
					//Debug.Log("A_ NAME = " + asmName.Name);

					var asmDllName = asmName.Name + ".dll";
					//Debug.Log("ASMDLLNAME = " + asmDllName);

					var asmFile = assemblies.FirstOrDefault(a => a.Name == asmDllName);
					//Debug.Log("ASM FILE = " + asmFile?.FullName);

					if (asmFile != null)
					{
						var processedBundleLocation = PostProcessBundle(bundle, assemblyReplacements);
						//Debug.Log($"Embedding {processedBundleLocation} into {asmFile.FullName}");
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
							//Debug.Log($"Embedding {processedBundleLocation} into {asmFile.FullName}");
							lock (embedLock)
							{
								EmbedResourceCMD.EmbedResource(asmFile.FullName, processedSceneBundleLocation, sceneBundle.File.Name + PlatformUtilities.GetBuildTargetExtension(sceneBundle.Target), compression: WeaverBuildTools.Enums.CompressionMethod.NoCompression);
							}
						}
					}
				}
				//var assembly = assemblies.FirstOrDefault(a => a.Name == bundle.)
			}
			//Debug.Log("END");
		}


		static void EmbedWeaverCoreResources()
		{
			var weaverGameLocation = new FileInfo(BuildTools.WeaverCoreFolder.AddSlash() + $"Other Projects~{Path.DirectorySeparatorChar}WeaverCore.Game{Path.DirectorySeparatorChar}WeaverCore.Game{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}WeaverCore.Game.dll");
			var harmonyLocation = new FileInfo(BuildTools.WeaverCoreFolder.AddSlash() + $"Libraries{Path.DirectorySeparatorChar}0Harmony.dll");
			var iLGenLocation = new FileInfo(BuildTools.WeaverCoreFolder.AddSlash() + $"Libraries{Path.DirectorySeparatorChar}System.Reflection.Emit.ILGeneration.dll");
			var emitLocation = new FileInfo(BuildTools.WeaverCoreFolder.AddSlash() + $"Libraries{Path.DirectorySeparatorChar}System.Reflection.Emit.dll");
			var emitLightweightLocation = new FileInfo(BuildTools.WeaverCoreFolder.AddSlash() + $"Libraries{Path.DirectorySeparatorChar}System.Reflection.Emit.Lightweight.dll");

			EmbedResourceCMD.EmbedResource(Data.WeaverCoreDLL, weaverGameLocation.FullName, "WeaverCore.Game", compression: CompressionMethod.NoCompression);
			EmbedResourceCMD.EmbedResource(Data.WeaverCoreDLL, harmonyLocation.FullName, "0Harmony", compression: CompressionMethod.NoCompression);
			//EmbedResourceCMD.EmbedResource(Data.WeaverCoreDLL, iLGenLocation.FullName, "ILGeneration", compression: CompressionMethod.NoCompression);
			//EmbedResourceCMD.EmbedResource(Data.WeaverCoreDLL, emitLocation.FullName, "ReflectionEmit", compression: CompressionMethod.NoCompression);
			//EmbedResourceCMD.EmbedResource(Data.WeaverCoreDLL, emitLightweightLocation.FullName, "ReflectionEmitLightweight", compression: CompressionMethod.NoCompression);
		}

		static Dictionary<string, AssemblyName> GetBundleToAssemblyPairs(Dictionary<string, string> assemblyReplacements)
		{
			Dictionary<string, AssemblyName> bundleToAssemblyPairs = new Dictionary<string, AssemblyName>();
			//var registryIDs = AssetDatabase.FindAssets($"t:{nameof(Registry)}");

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (var registry in Data.Registries)
			{
				//Debug.Log("Registry = " + JsonUtility.ToJson(registry, true));
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
					/*if (asmName.Name == "Assembly-CSharp" && !Data.WeaverCoreOnly)
					{
						asmName.Name = Data.ModName;
					}*/
					//Debug.Log($"Adding Bundle Pair {bundleName} => {asmName.Name}");
					bundleToAssemblyPairs.Add(registry.AssetBundleName, asmName);
					/*var mod = registry.ModType;
					if (mod != null)
					{
						assembly = mod.Assembly;
					}
					if (assembly != null)
					{
						
					}*/
				}
			}

			/*foreach (var id in registryIDs)
			{
				//Debug.Log("ID = " + id);
				//Debug.Log("Path = " + AssetDatabase.GUIDToAssetPath(id));
				var registry = AssetDatabase.LoadAssetAtPath<Registry>(AssetDatabase.GUIDToAssetPath(id));
				//Debug.Log("Registry = " + registry);
				//Debug.Log("Type = " + registry?.GetType());
				var bundleName = GetAssetBundleName(registry);
				//Assembly assembly = null;
				//registry.ModAssemblyName;
				//var modAssemblyName = registry.ModAssemblyName;
				//var assemblyName = new AssemblyName(registry.ModAssemblyName);
				//bundleToAssemblyPairs.Add(bundleName, assemblyName);

				var originalModName = registry.ModAssemblyName;
#if REWRITE_REGISTRIES
				foreach (var replacement in assemblyReplacements)
				{
					if (registry.ModAssemblyName == replacement.Value)
					{
						originalModName = replacement.Key;
						break;
					}
				}
#endif


				var assembly = assemblies.FirstOrDefault(a => a.GetName().Name == originalModName);
				if (assembly != null)
				{
					var asmName = assembly.GetName();
					asmName.Name = registry.ModAssemblyName;
#if !REWRITE_REGISTRIES
					if (assemblyReplacements.TryGetValue(asmName.Name,out var replacement))
					{
						asmName.Name = replacement;
					}
#endif
					//Debug.Log($"Adding Bundle Pair {bundleName} => {asmName.Name}");
					bundleToAssemblyPairs.Add(bundleName, asmName);
				}
				
			}*/

			return bundleToAssemblyPairs;



			/*string GetAssetBundleName(UnityEngine.Object obj)
			{
				var path = AssetDatabase.GetAssetPath(obj);
				if (path != null && path != "")
				{
					var import = AssetImporter.GetAtPath(path);
					return import.assetBundleName;
					//import.SetAssetBundleNameAndVariant(bundleName, import.assetBundleVariant);
				}
				return "";
			}*/
		}

		static string GetAssetBundleName(UnityEngine.Object obj)
		{
			var path = AssetDatabase.GetAssetPath(obj);
			if (path != null && path != "")
			{
				var import = AssetImporter.GetAtPath(path);
				return import.assetBundleName;
				//import.SetAssetBundleNameAndVariant(bundleName, import.assetBundleVariant);
			}
			return "";
		}

		static void DoneWithAssetBundling(MethodInfo whenFinished)
		{
			//Debug.Log("Done with Asset Bundling");
			//UnboundCoroutine.Start(Routine());
			//IEnumerator Routine()
			//{
			/*yield return new WaitForSeconds(0.5f);
			foreach (var registry in RegistryChecker.LoadAllRegistries())
			{
				registry.ReplaceAssemblyName(Data.ModName, "Assembly-CSharp");
				registry.ApplyChanges();
			}*/
			//Debug.Log("G_Editor File Locked = " + IsFileLocked(new FileInfo("Library\\ScriptAssemblies\\WeaverCore.Editor.dll")));
			//yield return new WaitForSeconds(0.5f);
			//yield return new WaitUntil(() => !EditorApplication.isCompiling);
			//Debug.Log("H_Editor File Locked = " + IsFileLocked(new FileInfo("Library\\ScriptAssemblies\\WeaverCore.Editor.dll")));
			//AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
			//Debug.Log("I_Editor File Locked = " + IsFileLocked(new FileInfo("Library\\ScriptAssemblies\\WeaverCore.Editor.dll")));
			//yield return new WaitForSeconds(0.5f);
			//Debug.Log("J_Editor File Locked = " + IsFileLocked(new FileInfo("Library\\ScriptAssemblies\\WeaverCore.Editor.dll")));
			//yield return new WaitUntil(() => !EditorApplication.isCompiling);
#if REWRITE_REGISTRIES
			foreach (var registry in RegistryChecker.LoadAllRegistries())
			{
				registry.ReplaceAssemblyName(Data.ModName, "Assembly-CSharp");
				registry.ApplyChanges();
			}
#endif
			//yield return new WaitForSeconds(0.5f);
			//Debug.Log("K_Editor File Locked = " + IsFileLocked(new FileInfo("Library\\ScriptAssemblies\\WeaverCore.Editor.dll")));
			//yield return new WaitUntil(() => !EditorApplication.isCompiling);
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
							AssetDatabase.ImportAsset(asmDef.AssemblyDefinitionPath, ImportAssetOptions.DontDownloadFromCacheServer);
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
				//if (assetsChanged)
				//{
				//	AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
				//}
				//Debug.Log("L_Editor File Locked = " + IsFileLocked(new FileInfo("Library\\ScriptAssemblies\\WeaverCore.Editor.dll")));
				//AssetDatabase.AllowAutoRefresh();
				AssetDatabase.StopAssetEditing();
			}
			//}
		}

		static void CompleteBundlingProcess()
		{
			//Debug.Log("M_Editor File Locked = " + IsFileLocked(new FileInfo("Library\\ScriptAssemblies\\WeaverCore.Editor.dll")));
			if (!Data.BundlingSuccessful)
			{
				Debug.LogError("An error occured when creating the asset bundles");
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
			/*UnboundCoroutine.Start(LoadScenes(() =>
			{
				
			}));*/
		}

		static string PostProcessBundle(BuiltAssetBundle bundle, Dictionary<string, string> assemblyReplacements)
		{
			Debug.Log($"Post Processing Bundle -> {bundle.File}");
			var am = new AssetsManager();
			am.LoadClassPackage(BuildTools.WeaverCoreFolder.AddSlash() + "Libraries\\classdata.tpk");

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
				Debug.Log("Assets File Name = " + assetsFileName);

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
								//Debug.Log($"Replacing in Monoscript {assemblyName} -> {newAsmName}");
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

						//If this MonoBehaviour has a field called "modAssemblyName", then it's a Registry object
						//If MonoBehaviour is a registry, replace the "modAssemblyName" variable from "Assembly-CSharp"
						if (!monoBehaviourInst.Get("modAssemblyName").IsDummy())
						{
							bool modified = false;
							var modAsmNameVal = monoBehaviourInst.Get("modAssemblyName").GetValue();
							foreach (var replacement in assemblyReplacements)
							{
								if (modAsmNameVal.AsString() == replacement.Key)
								{
									modAsmNameVal.Set(replacement.Value);
									Debug.Log($"Replacing Registry Assembly From {replacement.Key} to {replacement.Value}");
									modified = true;
									//assetReplacers.Add(new AssetsReplacerFromMemory(0, info.index, (int)info.curFileType, AssetHelper.GetScriptIndex(assetsFileInst.file, info), monoBehaviourInst.WriteToByteArray()));
									break;
								}
							}

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
										Debug.Log($"Replacing Registry Assembly From {replacement.Key} to {replacement.Value}");
										modified = true;
										//assetReplacers.Add(new AssetsReplacerFromMemory(0, info.index, (int)info.curFileType, AssetHelper.GetScriptIndex(assetsFileInst.file, info), monoBehaviourInst.WriteToByteArray()));
										break;
									}
								}
							}
							if (modified)
							{
								assetReplacers.Add(new AssetsReplacerFromMemory(0, info.index, (int)info.curFileType, AssetHelper.GetScriptIndex(assetsFileInst.file, info), monoBehaviourInst.WriteToByteArray()));
							}
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
