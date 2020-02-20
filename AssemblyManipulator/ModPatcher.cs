using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssemblyManipulator
{
	public sealed class ModPatcher : IDisposable
	{
		bool disposed = false;

		List<AssemblyDefinition> Assemblies = new List<AssemblyDefinition>();
		List<Stream> Streams = new List<Stream>();
		AssemblyDefinition ModAssembly;

		public ModPatcher(string modAssemblyPath,params string[] otherAssemblies)
		{
			ModAssembly = AssemblyDefinition.ReadAssembly(modAssemblyPath, new ReaderParameters() { ReadWrite = true, AssemblyResolver = new Resolver(this) });
			Assemblies.Add(ModAssembly);
			foreach (var assembly in otherAssemblies)
			{
				Assemblies.Add(AssemblyDefinition.ReadAssembly(assembly, new ReaderParameters() { AssemblyResolver = new Resolver(this) }));
			}
			for (int i = Assemblies.Count - 1; i >= 0; i--)
			{
				var assembly = Assemblies[i];
				var weaverCoreGameResource = assembly.MainModule.Resources.FirstOrDefault(r => r.Name == "WeaverCore.Game");
				if (weaverCoreGameResource != null && weaverCoreGameResource is EmbeddedResource embed)
				{
					var stream = embed.GetResourceStream();
					var gameAssembly = AssemblyDefinition.ReadAssembly(stream);
					Assemblies.Add(gameAssembly);
					Streams.Add(stream);
				}
			}
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					if (!Assemblies.Any(a => a.FullName == assembly.FullName) && assembly.Location != "")
					{
						Assemblies.Add(AssemblyDefinition.ReadAssembly(assembly.Location, new ReaderParameters() { AssemblyResolver = new Resolver(this) }));
					}
				}
				catch (NotSupportedException e)
				{
					if (!e.Message.Contains("not supported in a dynamic module"))
					{
						throw;
					}
				}
			}
		}

		TypeDefinition FindType(string fullName)
		{
			foreach (var assembly in Assemblies)
			{
				var foundType = assembly.MainModule.Types.FirstOrDefault(t => t.FullName == fullName);
				if (foundType != null)
				{
					return foundType;
				}
			}
			return null;
		}

		public void Patch(string nameSpace, string typeName,string weaverModType,string weaverModName)
		{
			//var modTemplate = FindType("WeaverCore.Game.ModTemplate");
			var duplicator = new TypeDuplicator(FindType("WeaverCore.Game.ModTemplate"), ModAssembly);

			duplicator.AddTypeReplacement(FindType("WeaverCore.Game.WeaverModTemplate"), FindType(weaverModType));
			duplicator.AddStringReplacement("MODNAMEHERE", weaverModName);

			duplicator.Create(nameSpace, typeName);

			ModAssembly.Write();
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
				foreach (var stream in Streams)
				{
					stream.Dispose();
				}
				GC.SuppressFinalize(this);
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
				return Resolve(name, new ReaderParameters());
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
				/*for (int i = patcher.Assemblies.Count - 1; i >= 0; i--)
				{
					var assembly = patcher.Assemblies[i];
					var weaverCoreGameResource = assembly.MainModule.Resources.FirstOrDefault(r => r.Name == "WeaverCore.Game");
					if (weaverCoreGameResource != null && weaverCoreGameResource is EmbeddedResource embed)
					{
						using (var stream = embed.GetResourceStream())
						{
							var gameAssembly = AssemblyDefinition.ReadAssembly(stream);
							patcher.Assemblies.Add(gameAssembly);
							if (gameAssembly.FullName == name.FullName)
							{
								return gameAssembly;
							}
						}
					}
				}*/
				return null;
			}
		}
	}
}
