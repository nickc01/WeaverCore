using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace WeaverCore.Editor.Compilation
{
	public static class MonoScriptExtensions
	{
		static Type monoScriptType;
		static MethodInfo M_GetAssemblyName;
		static MethodInfo M_GetNamespace;


		static MonoScriptExtensions()
		{
			monoScriptType = typeof(MonoScript);
			M_GetAssemblyName = monoScriptType.GetMethod("GetAssemblyName", BindingFlags.NonPublic | BindingFlags.Instance);
			M_GetNamespace = monoScriptType.GetMethod("GetNamespace", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		public static string GetScriptAssemblyName(this MonoScript script)
		{
			if (Application.unityVersion.StartsWith("2017"))
			{
				var parentDirectory = new DirectoryInfo("Assets").Parent;
				var path = AssetDatabase.GetAssetPath(script);
				DirectoryInfo currentDirectory = new FileInfo(path).Directory;

				while (!(currentDirectory.Name == "Assets" || currentDirectory.Name == "Editor"))
				{
					//var asmDefs = currentDirectory.
				}
				if (currentDirectory.Name == "Assets")
				{
					return "Assembly-CSharp.dll";
				}
				else if (currentDirectory.Name == "Editor")
				{
					return "Assembly-CSharp-Editor.dll";
				}
				return null;
			}
			else
			{
				return (string)M_GetAssemblyName.Invoke(script, null);
			}
		}

		public static string GetScriptNamespace(this MonoScript script)
		{
			return (string)M_GetNamespace.Invoke(script, null);
		}
	}
}