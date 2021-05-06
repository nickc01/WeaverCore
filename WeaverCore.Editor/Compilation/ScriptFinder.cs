using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace WeaverCore.Editor.Compilation
{
	[Serializable]
	public class AssemblyInformation
	{
		public string AssemblyName;
		public AssemblyDefinitionFile Definition;
		public string AssemblyDefinitionPath;
		public string AssemblyGUID;
		public List<string> ScriptPaths;
	}

	public static class ScriptFinder
	{
		static List<AssemblyInformation> infoCache;

		public static List<AssemblyInformation> GetProjectScriptInfo()
		{
			List<AssemblyInformation> AssemblyInfo = new List<AssemblyInformation>();
			AssemblyInfo = new List<AssemblyInformation>();

			foreach (var pair in AssemblyDefinitionFile.GetAllDefinitionsInFolder("Assets"))
			{
				AssemblyInfo.Add(new AssemblyInformation
				{
					AssemblyName = pair.Value.name,
					AssemblyDefinitionPath = pair.Key,
					AssemblyGUID = AssetDatabase.AssetPathToGUID(pair.Key).ToString(),
					Definition = pair.Value,
					ScriptPaths = new List<string>()
				});
			}

			AssemblyInfo.Add(new AssemblyInformation
			{
				Definition = null,
				AssemblyDefinitionPath = "",
				AssemblyGUID = "",
				AssemblyName = "Assembly-CSharp",
				ScriptPaths = new List<string>()
			});

			AssemblyInfo.Add(new AssemblyInformation
			{
				Definition = null,
				AssemblyDefinitionPath = "",
				AssemblyGUID = "",
				AssemblyName = "Assembly-CSharp-Editor",
				ScriptPaths = new List<string>()
			});

			var scriptIDs = AssetDatabase.FindAssets("t:MonoScript", new string[] { "Assets" });
			foreach (var id in scriptIDs)
			{
				var path = AssetDatabase.GUIDToAssetPath(id);
				var scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
				var scriptAssembly = scriptAsset.GetScriptAssemblyName().Replace(".dll", "");

				AssemblyInfo.First(asmInfo => asmInfo.AssemblyName == scriptAssembly).ScriptPaths.Add(path);
			}
			return AssemblyInfo;
		}

		/// <summary>
		/// Returns a list of all the scripts that are a part of the specified assembly. Returns null if the assembly name is not valid or if the assembly is precompiled
		/// </summary>
		public static List<string> FindAssemblyScripts(string assemblyName, List<AssemblyInformation> asmInfo = null)
		{
			if (asmInfo == null)
			{
				if (infoCache == null)
				{
					infoCache = GetProjectScriptInfo();
				}
				asmInfo = infoCache;
			}
			int index = asmInfo.FindIndex(info => info.AssemblyName == assemblyName);
			if (index == -1)
			{
				return null;
			}
			else
			{
				return asmInfo[index].ScriptPaths;
			}
		}

		/// <summary>
		/// Returns a list of all the scripts that are a part of the specified assembly. Returns null if the assembly name is not valid or if the assembly is precompiled
		/// </summary>
		public static List<string> FindAssemblyScripts(Assembly assembly, List<AssemblyInformation> asmInfo = null)
		{
			return FindAssemblyScripts(assembly.GetName().Name, asmInfo);
		}
	}
}
