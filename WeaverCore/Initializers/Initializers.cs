using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Initializers
{
	internal static class Initializers
	{
		internal static void OnGameInitialize() //This is only called when Hollow Knight starts up
		{
			if (CoreInfo.LoadState == Enums.RunningState.Game)
			{
				OnInitRunner.RunInitFunctions();
			}
		}

#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		static void OnEditorInitialize() //This is only called when the editor initializes
		{
			OnInitRunner.RunInitFunctions();
		}
#endif

#if UNITY_EDITOR
		[RuntimeInitializeOnLoadMethod]
#else
		[OnInit(int.MaxValue)]
#endif
		static void OnGamePlay() //This is called either when you go into play mode in the editor, or start up the game when in Hollow Knight
		{
			RuntimeInitRunner.RuntimeInit();
		}
	}


	static class OnInitRunner
	{
		static HashSet<Assembly> InitializedAssemblies = new HashSet<Assembly>();

		static bool run = false;

		public static void RunInitFunctions()
		{
			if (!run)
			{
				run = true;

				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					InitializedAssemblies.Add(assembly);
				}

				ReflectionUtilities.ExecuteMethodsWithAttribute<OnInitAttribute>();

				AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
			}
		}

		private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			if (!InitializedAssemblies.Contains(args.LoadedAssembly))
			{
				InitializedAssemblies.Add(args.LoadedAssembly);
				ReflectionUtilities.ExecuteMethodsWithAttribute<OnInitAttribute>(args.LoadedAssembly);
			}
		}
	}

	static class OnHarmonyPatchRunner
	{
		[OnInit(int.MaxValue)]
		static void OnInit()
		{
			foreach (var assembly in AppDomain.CurrentDomain.AllAssemblies())
			{
				PatchAssembly(assembly);
			}
			AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
		}

		private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			PatchAssembly(args.LoadedAssembly);
		}

		static void PatchAssembly(Assembly assembly)
		{

			var patcherInstance = HarmonyPatcher.Create("com." + assembly.GetName().Name + ".patch");

			var patchParameters = new Type[] { typeof(HarmonyPatcher) };
			var patchArguments = new object[] { patcherInstance };

			var inits = ReflectionUtilities.GetMethodsWithAttribute<OnHarmonyPatchAttribute>(assembly, patchParameters).ToList();
			inits.Sort(new PriorityAttribute.PrioritySorter<OnHarmonyPatchAttribute>());

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
	}

	static class RuntimeInitRunner
	{
		static HashSet<Assembly> InitializedAssemblies = new HashSet<Assembly>();

		static bool run = false;

		public static void RuntimeInit()
		{
			if (!run)
			{
				run = true;

				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					InitializedAssemblies.Add(assembly);
				}

				ReflectionUtilities.ExecuteMethodsWithAttribute<OnRuntimeInitAttribute>();

				AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
			}
		}

		private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			if (!InitializedAssemblies.Contains(args.LoadedAssembly))
			{
				InitializedAssemblies.Add(args.LoadedAssembly);
				ReflectionUtilities.ExecuteMethodsWithAttribute<OnRuntimeInitAttribute>(args.LoadedAssembly);
			}
		}
	}
}
