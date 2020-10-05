using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WeaverCore.Attributes;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Internal
{
	static class PatchRunner
	{
		//static HashSet<Assembly> PatchedAssemblies = new HashSet<Assembly>();

		[OnInit(int.MaxValue)]
		static void OnInit()
		{
			//Debugger.Log("PATCHING ALL ASSEMBLIES = " + new System.Diagnostics.StackTrace(true));

			//var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (var assembly in AppDomain.CurrentDomain.AllAssemblies())
			{
				PatchAssembly(assembly);
			}
			AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

			//In case some assemblies were loaded during this part and weren't in the list of assemblies to begin with

			/*var postAssemblies = AppDomain.CurrentDomain.GetAssemblies().Except(assemblies);
			foreach (var assembly in postAssemblies)
			{
				PatchAssembly(assembly);
			}*/
		}

		private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			PatchAssembly(args.LoadedAssembly);
		}

		static void PatchAssembly(Assembly assembly)
		{
			//try
			//{
			//if (PatchedAssemblies.Contains(assembly))
			//{
			//	return;
			//}

			//PatchedAssemblies.Add(assembly);

			var patcherInstance = HarmonyPatcher.Create("com." + assembly.GetName().Name + ".patch");

			//var patcher = (IPatch)Activator.CreateInstance(type);

			var patchParameters = new Type[] { typeof(HarmonyPatcher) };
			var patchArguments = new object[] { patcherInstance };

			var inits = ReflectionUtilities.GetMethodsWithAttribute<OnHarmonyPatchAttribute>(assembly, patchParameters).ToList();
			//var numberComparer = Comparer<int>.Default;
			//inits.Sort((a, b) => numberComparer.Compare(a.Item2.Priority, b.Item2.Priority));
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
}
