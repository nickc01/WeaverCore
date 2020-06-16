using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WeaverCore.Utilities;

namespace WeaverCore.Interfaces
{
	public interface IPatch
	{
		void Patch(HarmonyPatcher patcher);
	}

	class IPatchRunner : IInit
	{
		public void OnInit()
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				PatchAssembly(assembly);
			}

			AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
		}

		private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			PatchAssembly(args.LoadedAssembly);
		}

		void PatchAssembly(Assembly assembly)
		{
			foreach (var type in assembly.GetTypes().Where(t => typeof(IPatch).IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericTypeDefinition))
			{
				var patcher = (IPatch)Activator.CreateInstance(type);

				var patcherInstance = HarmonyPatcher.Create($"com.{assembly.GetName().Name}.patch");

				try
				{
					patcher.Patch(patcherInstance);
				}
				catch (Exception e)
				{
					Debugger.LogError("Patch Error: " + e);
				}
			}
		}
	}
}
