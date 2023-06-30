using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using WeaverCore.Assets;
using WeaverCore.Attributes;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore
{
	/// <summary>
	/// The main class used for initializing WeaverCore
	/// </summary>
    public static class Initialization
	{
		/// <summary>
		/// Whether WeaverCore has been initialized or not
		/// </summary>
		public static bool WeaverCoreInitialized { get; private set; }

		/// <summary>
		/// Whether the runtime aspects of WeaverCore have been initialized. Only set to true in hollow knight or when play mode is entered
		/// </summary>
		public static bool WeaverCoreRuntimeInitialized { get; private set; }

		/// <summary>
		/// The environment the mod is currently loaded in
		/// </summary>
#if UNITY_EDITOR
		public static RunningState Environment = RunningState.Editor;
#else
		public const RunningState Environment = RunningState.Game;
#endif

#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		static void OnEditorInitialize() //This is only called when the editor initializes
		{
			Initialize();
		}

		[UnityEngine.RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		static void OnRuntimeInitialize()
		{
			Initialize();
		}
#endif

		/// <summary>
		/// Initializes all the necessary components of WeaverCore
		/// </summary>
		public static void Initialize()
		{
			if (!WeaverCoreInitialized)
			{
				WeaverCoreInitialized = true;

#if !UNITY_EDITOR
				if (Application.isPlaying)
				{
					ReplaceFonts();
                }
#endif

#if UNITY_EDITOR
				LoadAsmIfNotFound("WeaverCore.Editor");
#else
				LoadAsmIfNotFound("WeaverCore.Game");
#endif
				ReflectionUtilities.ExecuteMethodsWithAttribute<OnInitAttribute>();

				foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
				{
					PatchAssembly(asm);
				}
			}

			if (!WeaverCoreRuntimeInitialized && Application.isPlaying)
			{
				WeaverCoreRuntimeInitialized = true;

				if (Application.isPlaying)
				{
					ReflectionUtilities.ExecuteMethodsWithAttribute<OnRuntimeInitAttribute>();
				}
			}
		}

		static void ReplaceFonts()
		{
			try
			{
				FontAssetContainer.InGameFonts = new HashSet<TMP_FontAsset>();

				var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();


				foreach (var gm in currentScene.GetRootGameObjects())
				{
					CheckGMForFonts(gm);
				}

				foreach (var gm in Resources.FindObjectsOfTypeAll<GameObject>())
				{
					CheckGMForFonts(gm);
				}

				foreach (var font in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
				{
					AddFont(font);
				}

				foreach (var gm in GameObject.FindObjectsOfType<GameObject>())
				{
					CheckGMForFonts(gm);
				}
			}
			catch (Exception e)
			{
				WeaverLog.Log("ERROR IN REPLACEFONTS");
				WeaverLog.LogException(e);
			}
		}

		static bool fontsReplaced = false;

		[OnRegistryLoad]
		static void OnRegistryLoad(Registry registry)
		{
			if (!fontsReplaced && Environment == RunningState.Game)
			{
				fontsReplaced = true;

                var fontAssetContainer = FontAssetContainer.Load();

                fontAssetContainer.ReplaceFonts();
            }
        }

        static void CheckGMForFonts(GameObject gm)
        {
			foreach (var tmp in gm.GetComponents<TextMeshPro>())
			{
				AddFont(tmp.font);
			}

			for (int i = 0; i < gm.transform.childCount; i++)
			{
				CheckGMForFonts(gm.transform.GetChild(i).gameObject);
			}
        }

        static void AddFont(TMP_FontAsset font)
        {
            if (font != null)
            {
				FontAssetContainer.InGameFonts.Add(font);
            }

            foreach (var fallback in font.fallbackFontAssets)
            {
				if (fallback != null && !FontAssetContainer.InGameFonts.Contains(fallback))
				{
                    AddFont(fallback);
                }
            }
        }

        static void PatchAssembly(Assembly assembly)
		{
			var patcherInstance = HarmonyPatcher.Create("com." + assembly.GetName().Name + ".patch");

			var patchParameters = new Type[] { typeof(HarmonyPatcher) };
			var patchArguments = new object[] { patcherInstance };

			var inits = ReflectionUtilities.GetMethodsWithAttribute<OnHarmonyPatchAttribute>(assembly, patchParameters).ToList();
			inits.Sort(new PriorityAttribute.MethodSorter<OnHarmonyPatchAttribute>());

			foreach (var initPair in ReflectionUtilities.GetMethodsWithAttribute<OnHarmonyPatchAttribute>(assembly, patchParameters))
			{
				try
				{
					initPair.Item1.Invoke(null, patchArguments);
				}
				catch (Exception e)
				{
					WeaverLog.LogError("Patch Error: " + e);
				}
			}
		}

		static Assembly LoadAsmIfNotFound(string assemblyName)
		{
			var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
			if (assembly == null)
			{
				assembly = ResourceUtilities.LoadAssembly(assemblyName);
			}
			return assembly;
		}

		[OnHarmonyPatch]
		static void PatchModLoader(HarmonyPatcher patcher)
		{
			if (Environment != RunningState.Game)
			{
				return;
			}

			var loadModM = typeof(IMod).Assembly.GetType("Modding.ModLoader").GetMethod("LoadMod", BindingFlags.NonPublic | BindingFlags.Static);

			var loadMod_postfix = typeof(Initialization).GetMethod(nameof(LoadMods_Postfix), BindingFlags.NonPublic | BindingFlags.Static);

			patcher.Patch(loadModM, null, loadMod_postfix);

			var unloadModM = typeof(IMod).Assembly.GetType("Modding.ModLoader").GetMethod("UnloadMod", BindingFlags.NonPublic | BindingFlags.Static);

			var unloadMod_postfix = typeof(Initialization).GetMethod(nameof(UnloadMods_Postfix), BindingFlags.NonPublic | BindingFlags.Static);

			patcher.Patch(unloadModM, null, unloadMod_postfix);
		}

		static void LoadMods_Postfix(object mod)
        {
			var modInstanceT = mod.GetType();

			var errorState = modInstanceT.GetField("Error").GetValue(mod);

            if (errorState != null)
            {
				return;
            }

			var imod = modInstanceT.GetField("Mod").GetValue(mod);

            if (imod != null)
            {
				var methods = ReflectionUtilities.GetMethodsWithAttribute<AfterModLoadAttribute>().ToList();

				foreach (var method in methods)
				{
					try
					{
						if (method.method.IsStatic && method.attribute.ModType != null && method.attribute.ModType.IsAssignableFrom(imod.GetType()))
						{
							var parameters = method.method.GetParameters();
							if (parameters.GetLength(0) == 1 && parameters[0].ParameterType.IsAssignableFrom(imod.GetType()))
							{
								method.method.Invoke(null, new object[] { imod });
							}
							else
							{
								method.method.Invoke(null, null);
							}
						}
					}
					catch (Exception e)
					{
						WeaverLog.LogError($"Error running function : {method.method.DeclaringType.FullName}:{method.method.Name}");
						WeaverLog.LogException(e);
					}
				}
				//ReflectionUtilities.ExecuteMethodsWithAttribute<AfterModLoadAttribute>((_, a) => a.ModType.IsAssignableFrom(imod.GetType()));
			}
        }

		static void UnloadMods_Postfix(object mod)
        {
			var modInstanceT = mod.GetType();

			var errorState = modInstanceT.GetField("Error").GetValue(mod);

			if (errorState != null)
			{
				return;
			}

			var imod = modInstanceT.GetField("Mod").GetValue(mod);

			if (imod != null)
			{
				var methods = ReflectionUtilities.GetMethodsWithAttribute<AfterModUnloadAttribute>().ToList();

				foreach (var method in methods)
				{
					try
					{
						if (method.method.IsStatic && method.attribute.ModType != null && method.attribute.ModType.IsAssignableFrom(imod.GetType()))
						{
							var parameters = method.method.GetParameters();
							if (parameters.GetLength(0) == 1 && parameters[0].ParameterType.IsAssignableFrom(imod.GetType()))
							{
								method.method.Invoke(null, new object[] { imod });
							}
							else
							{
								method.method.Invoke(null, null);
							}
						}
					}
					catch (Exception e)
					{
						WeaverLog.LogError($"Error running function : {method.method.DeclaringType.FullName}:{method.method.Name}");
						WeaverLog.LogException(e);
					}
				}
				//ReflectionUtilities.ExecuteMethodsWithAttribute<AfterModUnloadAttribute>((_, a) => a.ModType.IsAssignableFrom(imod.GetType()));
			}
		}
	}
}
