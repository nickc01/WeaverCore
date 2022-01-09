using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace WeaverCore.Editor.Compilation
{
	/// <summary>
	/// Stores information about an assembly in the project
	/// </summary>
	[Serializable]
	public class AssemblyInformation
	{
		/// <summary>
		/// The name of the assembly
		/// </summary>
		public string AssemblyName;

		/// <summary>
		/// The asmdef file the assembly is defined in
		/// </summary>
		public AssemblyDefinitionFile Definition;

		/// <summary>
		/// The file path of the asmdef file
		/// </summary>
		public string AssemblyDefinitionPath;

		/// <summary>
		/// The GUID of the assembly
		/// </summary>
		public string AssemblyGUID;

		/// <summary>
		/// A list of all the script file paths that are a part of the assembly
		/// </summary>
		public List<string> ScriptPaths;

		/// <summary>
		/// Loads information about an assembly based on the <see cref="AssemblyDefinitionPath"/>
		/// </summary>
		public void Load()
		{
			Definition = AssemblyDefinitionFile.Load(AssemblyDefinitionPath);
		}

		/// <summary>
		/// Saves all current information to the asmdef file
		/// </summary>
		public void Save()
		{
			AssemblyDefinitionFile.Save(AssemblyDefinitionPath, Definition);
		}
	}

	/// <summary>
	/// Used for finding all scripts in a project, and other information about them
	/// </summary>
	public static class ScriptFinder
	{
		static List<AssemblyInformation> infoCache;

		/// <summary>
		/// Gets all information about all scripts in the project
		/// </summary>
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
