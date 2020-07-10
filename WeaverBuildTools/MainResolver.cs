using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace WeaverBuildTools
{
	public sealed class MainResolver : IAssemblyResolver, IDisposable
	{
		bool disposed = false;
		

		//ModPatcher patcher;
		IEnumerable<AssemblyDefinition> Assemblies;

		public MainResolver()
		{
			List<AssemblyDefinition> AssemblyList = new List<AssemblyDefinition>();
			Assemblies = AssemblyList;
			foreach (var loadedAssembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				string location;
				if (TryGetLocation(loadedAssembly,out location) && File.Exists(location))
				{
					AssemblyList.Add(AssemblyDefinition.ReadAssembly(location, new ReaderParameters() { AssemblyResolver = this }));
				}
			}
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

		static bool TryGetLocation(Assembly assembly, out string location)
		{
			try
			{
				location = assembly.Location;
				return true;
			}
			catch (Exception)
			{
				location = null;
				return false;
			}
		}

		~MainResolver()
		{
			Dispose();
		}
	}
}