using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore
{
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
		/// Initializes all the necessary components of WeaverCore. Does nothing if the mod is already initialized
		/// </summary>
		public static void Initialize()
		{
			//UnityEditor.EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
			

			if (!WeaverCoreInitialized)
			{
				WeaverCoreInitialized = true;
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

			//Debug.Log("Playing Game = " + Application.isPlaying);
			if (!WeaverCoreRuntimeInitialized && Application.isPlaying)
			{
				WeaverCoreRuntimeInitialized = true;

				if (Application.isPlaying)
				{
					ReflectionUtilities.ExecuteMethodsWithAttribute<OnRuntimeInitAttribute>();
				}
			}
		}

		/*private static void EditorApplication_playModeStateChanged(UnityEditor.PlayModeStateChange obj)
		{
			Debug.Log("Play State = " + obj);
		}*/

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

		/*[OnInit]
		static void InitTest()
		{
			Debug.Log("Init Test");
		}

		static void RuntimeInitTest()
		{
			Debug.Log("Runtime Init Test");
		}*/
	}
}
