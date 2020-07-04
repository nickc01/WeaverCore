using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WeaverCore.Interfaces
{
	public interface IInit
	{
		void OnInit();
	}

	internal static class InitRunner
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
					RunInitFunctions(assembly);
				}
				AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
			}
		}

		private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			RunInitFunctions(args.LoadedAssembly);
		}

		static void RunInitFunctions(Assembly assembly)
		{
			try
			{
				if (InitializedAssemblies.Contains(assembly))
				{
					return;
				}

				InitializedAssemblies.Add(assembly);

				//Debugger.Log("RUnning Init Funcs for = " + assembly);

				//WeaverLog.Log("Initializing Assembly = " + assembly.GetName().Name);

				foreach (var type in assembly.GetTypes().Where(t => typeof(IInit).IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericTypeDefinition))
				{
					var init = (IInit)Activator.CreateInstance(type);
					try
					{
						init.OnInit();
					}
					catch (Exception e)
					{
						WeaverLog.LogError("Init Error: " + e);
					}
				}
			}
			/*catch (ReflectionTypeLoadException e)
			{
				//WeaverLog.LogError("Init Error: " + e);
				//WeaverLog.LogError("Types that loaded");
				//WeaverLog.LogError("[");
				foreach (var type in e.Types)
				{
					if (type == null)
					{
						WeaverLog.LogError("\tnull");
					}
					else
					{
						WeaverLog.Log("\t" + type.FullName);
					}
				}
				WeaverLog.Log("]");
			}*/
			catch (Exception e)
			{
				WeaverLog.LogError("Init Error: " + e);
			}
		}
	}
}
