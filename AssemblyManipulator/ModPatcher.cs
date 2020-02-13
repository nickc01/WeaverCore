using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssemblyManipulator
{
	public sealed class ModPatcher : IDisposable
	{
		bool disposed = false;

		List<AssemblyDefinition> Assemblies;
		AssemblyDefinition ModAssembly;

		public ModPatcher(string modAssemblyPath,params string[] otherAssemblies)
		{
			ModAssembly = AssemblyDefinition.ReadAssembly(modAssemblyPath, new ReaderParameters() { ReadWrite = true, AssemblyResolver = new Resolver(this) });
			Assemblies.Add(ModAssembly);
			foreach (var assembly in otherAssemblies)
			{
				Assemblies.Add(AssemblyDefinition.ReadAssembly(assembly, new ReaderParameters() { AssemblyResolver = new Resolver(this) }));
			}
		}

		public void Patch()
		{

		}

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;
				foreach (var assembly in Assemblies)
				{
					assembly.Dispose();
				}
			}
		}

		~ModPatcher()
		{
			Dispose();
		}


		class Resolver : IAssemblyResolver
		{
			ModPatcher patcher;

			public Resolver(ModPatcher Patcher)
			{
				patcher = Patcher;
			}

			public void Dispose()
			{
				
			}

			public AssemblyDefinition Resolve(AssemblyNameReference name)
			{
				foreach (var assembly in patcher.Assemblies)
				{
					if (assembly.FullName == name.FullName)
					{
						return assembly;
					}
				}
				return null;
			}

			public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
			{
				foreach (var assembly in patcher.Assemblies)
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
}
