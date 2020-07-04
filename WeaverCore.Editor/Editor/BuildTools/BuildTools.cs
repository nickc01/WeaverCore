using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using WeaverCore.Editor.Visual.Helpers;
using WeaverCore.Utilities;

namespace WeaverCore
{
	public static class BuildTools
	{
		static Assembly toolsAssembly;
		static Type compressionMethodT;

		static MethodInfo embedMethodA;
		static MethodInfo embedMethodB;
		static addModDelegate addModMethod;




		delegate void addModDelegate(string assemblyToAddTo, string typeNamespace, string typeName, string modName, bool unloadable, string hollowKnightEXEPath, string weaverCorePath);

		static BuildTools()
		{
			//Find the Assembly Loader Type
			var assemblyLoader = typeof(BuildTools).Assembly.GetType("Costura.AssemblyLoader");

			//Find the method that resolves the assembly
			var resolver = assemblyLoader.GetMethod("ResolveAssembly", BindingFlags.Public | BindingFlags.Static);

			//Resolve the assembly. Mono.Cecil should be loaded beyond this point
			resolver.Invoke(null, new object[] { null, new ResolveEventArgs("Mono.Cecil, Version=0.10.4.0, Culture=neutral, PublicKeyToken=50cebf1cceb9d05e") });

			toolsAssembly = AppDomain.CurrentDomain.Load("WeaverBuildTools");

			compressionMethodT = toolsAssembly.GetType("WeaverBuildTools.Enums.CompressionMethod");

			embedMethodA = toolsAssembly.GetType("WeaverBuildTools.Commands.EmbedResourceCMD").GetMethod("EmbedResource",new Type[] { typeof(string),typeof(string), typeof(string),typeof(string),compressionMethodT });
			embedMethodB = toolsAssembly.GetType("WeaverBuildTools.Commands.EmbedResourceCMD").GetMethod("EmbedResource",new Type[] { typeof(string),typeof(string), typeof(Stream),typeof(string),compressionMethodT });
			addModMethod = GetMethod<addModDelegate>("WeaverBuildTools.Commands.AddModCMD", "AddMod");
		}

		static F GetMethod<F>(string typeName, string methodName) where F : class
		{
			//return MethodUtilities.ConvertToDelegate<F>(GetMethod(typeName,methodName));
			var method = GetMethod(typeName, methodName);
			return Delegate.CreateDelegate(method.DeclaringType, method) as F;
		}

		static MethodInfo GetMethod(string typeName, string methodName)
		{
			var type = toolsAssembly.GetType(typeName);
			if (type == null)
			{
				throw new Exception("Type: " + typeName + " not found in WeaverTools");
			}
			var method = type.GetMethod(typeName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
			if (method == null)
			{
				throw new Exception("Could not find method of " + methodName + " on type: " + typeName);
			}
			return method;
		}

		public static void EmbedResource(string assemblyToEmbedTo, string resourceName, Stream streamToEmbed, string hash = null, CompressionMethod compression = CompressionMethod.Auto)
		{
			embedMethodB.Invoke(null, new object[] { assemblyToEmbedTo, resourceName, streamToEmbed, hash, EnumUtilities.RawConvert(compression,compressionMethodT) });
		}

		public static void EmbedResource(string assemblyToEmbedTo, string fileToEmbed, string resourceName, string hash = null, CompressionMethod compression = CompressionMethod.Auto)
		{
			embedMethodB.Invoke(null, new object[] { assemblyToEmbedTo, fileToEmbed, resourceName, hash, EnumUtilities.RawConvert(compression, compressionMethodT) });
		}

		public static void AddMod(string assemblyToAddTo, string typeNamespace, string typeName, string modName, bool unloadable, string hollowKnightEXEPath, string weaverCorePath)
		{
			addModMethod(assemblyToAddTo, typeNamespace, typeName, modName, unloadable, hollowKnightEXEPath, weaverCorePath);
		}
	}
}
