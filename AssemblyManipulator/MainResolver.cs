using Mono.Cecil;
using System.Collections.Generic;

namespace AssemblyManipulator
{
	public class MainResolver : IAssemblyResolver
	{
		//ModPatcher patcher;
		IEnumerable<AssemblyDefinition> Assemblies;

		public MainResolver(IEnumerable<AssemblyDefinition> assemblies)
		{
			Assemblies = assemblies;
		}

		public void Dispose()
		{

		}

		public AssemblyDefinition Resolve(AssemblyNameReference name)
		{
			return Resolve(name, new ReaderParameters());
		}

		public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
		{
			foreach (var assembly in Assemblies)
			{
				if (assembly.FullName == name.FullName)
				{
					return assembly;
				}
			}
			return null;
		}
	}
}
