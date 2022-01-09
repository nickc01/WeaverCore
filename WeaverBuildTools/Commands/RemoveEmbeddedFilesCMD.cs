using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverBuildTools.Commands
{
	public static class RemoveEmbeddedFilesCMD
	{
		public static void RemoveEmbeddedFiles(string assemblyToRemoveFrom)
		{
			using (var definition = AssemblyDefinition.ReadAssembly(assemblyToRemoveFrom, new ReaderParameters { ReadWrite = true }))
			{
				for (int i = definition.MainModule.Resources.Count - 1; i >= 0; i--)
				{
					var resource = definition.MainModule.Resources[i];
					if (resource != null && resource is EmbeddedResource)
					{
						definition.MainModule.Resources.Remove(resource);
					}
				}
				definition.MainModule.Write();
			}
		}

		public static string GetHelp()
		{
			return "-----removeembeddedfiles-----\n" +
					"Removes all resources stored in an assembly\n" +
					"\n" +
					"removeembeddedfiles {assemblyToRemoveFrom}\n\n" +
					"---------------------\n\n";
		}
	}
}
