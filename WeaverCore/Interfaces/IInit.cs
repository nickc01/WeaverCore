using System;
using System.Collections.Generic;
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
		public static void RunInitFunctions()
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				RunInitFunctions(assembly);
			}
			AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
		}

		private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			RunInitFunctions(args.LoadedAssembly);
		}

		public static void RunInitFunctions(Assembly assembly)
		{
			foreach (var type in assembly.GetTypes().Where(t => typeof(IInit).IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericTypeDefinition))
			{
				var init = (IInit)Activator.CreateInstance(type);
				try
				{
					init.OnInit();
				}
				catch (Exception e)
				{
					Debugger.LogError("Init Error: " + e);
				}
			}
		}
	}
}
