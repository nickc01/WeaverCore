using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
		public System.Collections.Generic.List<string> ScriptPaths;

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
		static System.Collections.Generic.List<string> GetScriptsForAsmDef(string asmDefPath)
		{
			System.Collections.Generic.List<string> results = new System.Collections.Generic.List<string>();

			void GatherScripts(DirectoryInfo dir, DirectoryInfo root)
			{
				// Skip any folders marked with "~" (e.g. "Other Projects~")
				if (dir.Name.EndsWith("~"))
				{
					return;
				}

				if (dir.FullName != root.FullName && dir.GetFiles("*.asmdef", SearchOption.TopDirectoryOnly).Length > 0)
				{
					return;
				}

				foreach (var file in dir.GetFiles("*.cs", SearchOption.TopDirectoryOnly))
				{
					results.Add(WeaverCore.Utilities.PathUtilities.ConvertToProjectPath(file.FullName));
				}

				foreach (var sub in dir.GetDirectories("*", SearchOption.TopDirectoryOnly))
				{
					GatherScripts(sub, root);
				}
			}

			var asmDefDir = new FileInfo(asmDefPath).Directory;
			GatherScripts(asmDefDir, asmDefDir);
			return results;
		}

		static System.Collections.Generic.List<AssemblyInformation> infoCache;

		/// <summary>
		/// Gets all information about all scripts in the project
		/// </summary>
		public static System.Collections.Generic.List<AssemblyInformation> GetProjectScriptInfo()
		{
            System.Collections.Generic.List<AssemblyInformation> AssemblyInfo = new System.Collections.Generic.List<AssemblyInformation>();
			AssemblyInfo = new System.Collections.Generic.List<AssemblyInformation>();

			foreach (var pair in AssemblyDefinitionFile.GetAllDefinitionsInFolder("Assets"))
			{
				AssemblyInfo.Add(new AssemblyInformation
                {
					AssemblyName = pair.Value.name,
					AssemblyDefinitionPath = pair.Key,
					AssemblyGUID = AssetDatabase.AssetPathToGUID(pair.Key).ToString(),
					Definition = pair.Value,
					ScriptPaths = GetScriptsForAsmDef(pair.Key)
				});
			}

			AssemblyInfo.Add(new AssemblyInformation
            {
				Definition = null,
				AssemblyDefinitionPath = "",
				AssemblyGUID = "",
				AssemblyName = "Assembly-CSharp",
				ScriptPaths = new System.Collections.Generic.List<string>()
			});

			AssemblyInfo.Add(new AssemblyInformation
            {
				Definition = null,
				AssemblyDefinitionPath = "",
				AssemblyGUID = "",
				AssemblyName = "Assembly-CSharp-Editor",
				ScriptPaths = new System.Collections.Generic.List<string>()
			});

			var scriptIDs = AssetDatabase.FindAssets("t:MonoScript", new string[] { "Assets" });
			var pendingScripts = new System.Collections.Generic.List<string>();
			var dirAssemblyMap = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (var id in scriptIDs)
			{
				var path = AssetDatabase.GUIDToAssetPath(id);
				var scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
				var scriptAssembly = scriptAsset.GetScriptAssemblyName();
				if (!string.IsNullOrEmpty(scriptAssembly))
				{
					scriptAssembly = scriptAssembly.Replace(".dll", "");
				}

				if (string.IsNullOrEmpty(scriptAssembly))
				{
					pendingScripts.Add(path);
					continue;
				}

				var foundAsm = AssemblyInfo.FirstOrDefault(asmInfo => asmInfo.AssemblyName == scriptAssembly);
				if (foundAsm != null)
				{
					foundAsm.ScriptPaths.Add(path);
                }
				var dir = Path.GetDirectoryName(path);
				if (!string.IsNullOrEmpty(dir) && !dirAssemblyMap.ContainsKey(dir))
				{
					dirAssemblyMap.Add(dir, scriptAssembly);
				}
			}

			if (pendingScripts.Count > 0)
			{
				foreach (var path in pendingScripts)
				{
					var dir = Path.GetDirectoryName(path);
					var resolvedAssembly = ResolveAssemblyFromDirectory(dir, dirAssemblyMap);
					if (!string.IsNullOrEmpty(resolvedAssembly))
					{
						var foundAsm = AssemblyInfo.FirstOrDefault(asmInfo => asmInfo.AssemblyName == resolvedAssembly);
						if (foundAsm != null)
						{
							foundAsm.ScriptPaths.Add(path);
						}
					}
					else
					{
						Debug.LogWarning($"Unable to resolve assembly for script at {path}");
					}
				}
			}
			return AssemblyInfo;
		}

		static string ResolveAssemblyFromDirectory(string startDir, System.Collections.Generic.Dictionary<string, string> dirAssemblyMap)
		{
			if (string.IsNullOrEmpty(startDir))
			{
				return null;
			}

			var current = startDir;
			while (!string.IsNullOrEmpty(current))
			{
				if (dirAssemblyMap.TryGetValue(current, out var assemblyName))
				{
					return assemblyName;
				}

				var parent = Path.GetDirectoryName(current);
				if (string.Equals(parent, current, StringComparison.OrdinalIgnoreCase))
				{
					break;
				}
				current = parent;
			}

			return null;
		}

		/// <summary>
		/// Returns a list of all the scripts that are a part of the specified assembly. Returns null if the assembly name is not valid or if the assembly is precompiled
		/// </summary>
		public static System.Collections.Generic.List<string> FindAssemblyScripts(string assemblyName, System.Collections.Generic.List<AssemblyInformation> asmInfo = null)
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
		public static System.Collections.Generic.List<string> FindAssemblyScripts(Assembly assembly, System.Collections.Generic.List<AssemblyInformation> asmInfo = null)
		{
			return FindAssemblyScripts(assembly.GetName().Name, asmInfo);
		}
	}
}
