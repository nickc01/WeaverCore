using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace WeaverCore.Editor.Compilation
{
	/// <summary>
	/// Several extension methods for working with <see cref="MonoScript"/> objects
	/// </summary>
	public static class MonoScriptExtensions
	{
		static Type monoScriptType;
#if UNITY_2019_4_OR_NEWER
		static MethodInfo M_GetAssemblyName;
		static MethodInfo M_GetNamespace;
#endif


		static MonoScriptExtensions()
		{
			monoScriptType = typeof(MonoScript);
#if UNITY_2019_4_OR_NEWER
			M_GetAssemblyName = monoScriptType.GetMethod("GetAssemblyName", BindingFlags.NonPublic | BindingFlags.Instance);
			M_GetNamespace = monoScriptType.GetMethod("GetNamespace", BindingFlags.NonPublic | BindingFlags.Instance);
#endif
		}

		/// <summary>
		/// Gets the assembly name a monoscript is a part of
		/// </summary>
		public static string GetScriptAssemblyName(this MonoScript script)
		{
#if UNITY_2019_4_OR_NEWER
			return (string)M_GetAssemblyName.Invoke(script, null);
#else
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
#endif
		}

		/// <summary>
		/// Gets the namespace the monoscript is a part of
		/// </summary>
		public static string GetScriptNamespace(this MonoScript script)
		{
#if UNITY_2019_4_OR_NEWER
			return (string)M_GetNamespace.Invoke(script, null);
#endif
		}
	}
}