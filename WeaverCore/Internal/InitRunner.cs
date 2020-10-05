using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Internal
{
	public static class InitRunner
	{
		static HashSet<Assembly> InitializedAssemblies = new HashSet<Assembly>();

		static bool run = false;

		public static void RunInitFunctions()
		{
			if (!run)
			{
				run = true;
				//var assemblies = AppDomain.CurrentDomain.GetAssemblies();
				/*foreach (var assembly in AppDomain.CurrentDomain.AllAssemblies())
				{
					RunInitFunctions(assembly);
				}*/

				//IEnumerable<ValueTuple<MethodInfo, OnInitAttribute>> InitFunctions = null;

				/*foreach (var assembly in AppDomain.CurrentDomain.AllAssemblies())
				{
					//WeaverLog.Log("Getting Funcs in " + assembly.FullName);
					var inits = GetInitFunctions(assembly);
					if (InitFunctions == null)
					{
						InitFunctions = inits;
					}
					else if (inits != null)
					{
						InitFunctions = InitFunctions.Concat(inits);
					}
				}*/

				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					//WeaverLog.Log("Running Init for Assembly = " + assembly.GetName().Name);
					InitializedAssemblies.Add(assembly);
				}

				ReflectionUtilities.ExecuteMethodsWithAttribute<OnInitAttribute>();

				//ExecuteFunctions(InitFunctions);
				AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
			}
		}

		private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			if (!InitializedAssemblies.Contains(args.LoadedAssembly))
			{
				InitializedAssemblies.Add(args.LoadedAssembly);
				//WeaverLog.Log("Running Init for Assembly = " + args.LoadedAssembly.GetName().Name);
				ReflectionUtilities.ExecuteMethodsWithAttribute<OnInitAttribute>(args.LoadedAssembly);
			}
		}
	}
}
