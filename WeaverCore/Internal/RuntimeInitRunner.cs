using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Implementations;
using System.Reflection;
using WeaverCore.Interfaces;
using WeaverCore.Enums;
using WeaverCore.Attributes;

namespace WeaverCore.Internal
{
	public static class RuntimeInitRunner
	{
		static HashSet<Assembly> InitializedAssemblies = new HashSet<Assembly>();


		static bool run = false;

		[RuntimeInitializeOnLoadMethod]
		public static void RuntimeInit()
		{
			if (!run)
			{
				run = true;

				//var assemblies = AppDomain.CurrentDomain.GetAssemblies();
				/*IEnumerable<ValueTuple<MethodInfo, OnRuntimeInitAttribute>> RuntimeInitFunctions = null;

				foreach (var assembly in AppDomain.CurrentDomain.AllAssemblies())
				{

					var inits = GetRuntimeFunctions(assembly);
					if (RuntimeInitFunctions == null)
					{
						RuntimeInitFunctions = inits;
					}
					else if (inits != null)
					{
						RuntimeInitFunctions = RuntimeInitFunctions.Concat(inits);
					}
				}

				ExecuteFunctions(RuntimeInitFunctions);*/

				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					InitializedAssemblies.Add(assembly);
					//WeaverLog.Log("Running RUNTIME Init for Assembly = " + assembly.GetName().Name);
				}

				ReflectionUtilities.ExecuteMethodsWithAttribute<OnRuntimeInitAttribute>();

				AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

				//In case some assemblies were loaded during this part and weren't in the list of assemblies to begin with

				/*var postAssemblies = AppDomain.CurrentDomain.GetAssemblies().Except(assemblies);
				foreach (var assembly in postAssemblies)
				{
					DoRuntimeInit(assembly);
				}*/
			}
		}

		private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			//ExecuteFunctions(GetRuntimeFunctions(args.LoadedAssembly));
			if (!InitializedAssemblies.Contains(args.LoadedAssembly))
			{
				InitializedAssemblies.Add(args.LoadedAssembly);
				//WeaverLog.Log("Running RUNTIME Init for Assembly = " + args.LoadedAssembly.GetName().Name);
				ReflectionUtilities.ExecuteMethodsWithAttribute<OnRuntimeInitAttribute>(args.LoadedAssembly);
			}
		}
	}
}
