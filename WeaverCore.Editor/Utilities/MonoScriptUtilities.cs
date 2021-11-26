using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Utilities
{
	public static class MonoScriptUtilities
	{
		class MonoScriptMeta
		{
			public MonoScript MonoScript;
			public string AssemblyName;
			public string OriginalAssemblyName;
			public string AssetPath;
			public string FileName;
		}


		static HashSet<MonoScriptMeta> monoScripts = new HashSet<MonoScriptMeta>();
		static DirectoryInfo projectFolder = new DirectoryInfo($"Assets{Path.DirectorySeparatorChar}..");


		static MethodInfo getNamespace = typeof(MonoScript).GetMethod("GetNamespace", BindingFlags.NonPublic | BindingFlags.Instance);
		static MethodInfo dynMethod = typeof(MonoScript).GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance);


		static MonoScriptUtilities()
		{
			string[] assetPaths = AssetDatabase.GetAllAssetPaths();
			foreach (var assetPath in assetPaths)
			{
				var fileInfo = new FileInfo(assetPath);
				var relativePath = PathUtilities.MakePathRelative(projectFolder.FullName, fileInfo.FullName);

				MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(relativePath);
				if (script != null)
				{
					var assemblyName = FindAssemblyDefinition(new FileInfo(assetPath));
					monoScripts.Add(new MonoScriptMeta()
					{
						MonoScript = script,
						AssemblyName = assemblyName,
						OriginalAssemblyName = assemblyName,
						AssetPath = relativePath,
						FileName = fileInfo.Name
					});

				}
			}
		}


		public static void ChangeAssemblyName(string fromAssemblyName, string toAssemblyName)
		{
			foreach (var script in monoScripts)
			{
				if (script.AssemblyName == fromAssemblyName)
				{
					ReInitMonoScript(script.MonoScript, toAssemblyName);
					script.AssemblyName = toAssemblyName;
				}
			}
		}


		static void ReInitMonoScript(MonoScript script, string newAssembly)
		{
			dynMethod.Invoke(script, new object[] { script.text, script.name, (string)getNamespace.Invoke(script, null), newAssembly, false });
		}


		public static MonoScript GetScriptAtPath(string assetPath)
		{
			assetPath = new FileInfo(assetPath).FullName;
			assetPath = PathUtilities.MakePathRelative(projectFolder.FullName, assetPath);

			foreach (var script in monoScripts)
			{
				if (script.AssetPath == assetPath)
				{
					return script.MonoScript;
				}
			}
			return null;
		}

		public static string GetScriptAssemblyName(MonoScript monoScript)
		{
			foreach (var script in monoScripts)
			{
				if (script.MonoScript == monoScript)
				{
					return script.AssemblyName;
				}
			}
			return "";
		}

		public static void SetScriptAssemblyName(MonoScript monoScript, string assemblyName)
		{
			foreach (var script in monoScripts)
			{
				if (script.MonoScript == monoScript)
				{
					ReInitMonoScript(script.MonoScript, assemblyName);
					script.AssemblyName = assemblyName;
					return;
				}
			}
		}

		public static IEnumerable<MonoScript> GetScriptsUnderAssembly(string assemblyName)
		{
			foreach (var script in monoScripts)
			{
				if (script.AssemblyName == assemblyName)
				{
					yield return script.MonoScript;
				}
			}
		}

		public static IEnumerable<FileInfo> GetScriptPathsUnderAssembly(string assemblyName)
		{
			foreach (var script in monoScripts)
			{
				if (script.AssemblyName == assemblyName)
				{
					yield return new FileInfo(script.AssetPath);
				}
			}
		}

		public static IEnumerable<FileInfo> GetAsmDefPaths()
		{
			return PathUtilities.AssetsFolder.GetFiles("*.asmdef", SearchOption.AllDirectories);
		}

		static string FindAssemblyDefinition(FileInfo filePath)
		{
			Regex nameFinder = new Regex(@"""name"": ""(.+?)""");

			bool editorFolder = false;

			var directory = filePath.Directory;
			while (directory.FullName != projectFolder.FullName)
			{
				if (directory.Name == "Editor")
				{
					editorFolder = true;
				}
				var asmDef = directory.GetFiles("*.asmdef", SearchOption.TopDirectoryOnly);
				if (asmDef.GetLength(0) > 0)
				{
					var contents = File.ReadAllText(asmDef[0].FullName);
					var nameMatch = nameFinder.Match(contents);
					if (nameMatch.Success)
					{
						return nameMatch.Groups[1].Value;
					}
				}
				directory = directory.Parent;
			}
			if (editorFolder)
			{
				return "Assembly-CSharp-Editor";
			}
			return "Assembly-CSharp";
		}
	}
}
