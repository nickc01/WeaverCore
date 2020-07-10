using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Implementations;
using System.Reflection;
using WeaverCore.Interfaces;

namespace WeaverCore.Internal
{
	public static class RuntimeInitializer
	{
		static HashSet<Assembly> InitializedAssemblies = new HashSet<Assembly>();

		[RuntimeInitializeOnLoadMethod]
		static void Load()
		{
			RuntimeInit();
		}

		static bool run = false;

		public static void RuntimeInit()
		{
			if (!run)
			{
				run = true;
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					DoRuntimeInit(assembly);
				}

				AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
			}
		}

		private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			DoRuntimeInit(args.LoadedAssembly);
		}

		static void DoRuntimeInit(Assembly assembly)
		{
			if (assembly.GetName().Name == "Assembly-CSharp")
			{
				return;
			}
			try
			{
				if (InitializedAssemblies.Contains(assembly))
				{
					return;
				}
				InitializedAssemblies.Add(assembly);

				foreach (var type in assembly.GetTypes().Where(t => typeof(IRuntimeInit).IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericTypeDefinition))
				{
					var rInit = (IRuntimeInit)Activator.CreateInstance(type);
					try
					{
						rInit.RuntimeInit();
					}
					catch (Exception e)
					{
						WeaverLog.LogError("Runtime Init Error: " + e);
					}
				}
			}
			catch  (Exception e)
			{
				WeaverLog.LogError("Error when attempting to runtime-initialize [" + assembly.GetName().Name + "] : " + e);
			}
		}
	}
}
