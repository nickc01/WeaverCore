using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace WeaverCore.Editor.Compilation
{
	/// <summary>
	/// Represents a loaded asmdef file
	/// </summary>
	[Serializable]
	public class AssemblyDefinitionFile
	{
		public string name;
		public string rootNamespace = "";
		public List<string> references;
		public List<string> includePlatforms;
		public List<string> excludePlatforms;
		public bool allowUnsafeCode = false;
		public bool overrideReferences = false;
		public bool autoReferenced = true;
		public List<string> defineConstraints;
		public bool noEngineReferences = false;

		[SerializeField]
		List<string> precompiledReferences;

		public static AssemblyDefinitionFile Load(string path)
		{
			var asmDefAsset = AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset)) as TextAsset;
			if (asmDefAsset != null)
			{
				return JsonUtility.FromJson<AssemblyDefinitionFile>(asmDefAsset.text);
			}

			return null;
		}

		public static void Save(string path, AssemblyDefinitionFile asmDef)
		{
			File.WriteAllText(path, JsonUtility.ToJson(asmDef,true));
		}

		public static Dictionary<string,AssemblyDefinitionFile> GetAllDefinitionsInFolder(string folder)
		{
			Dictionary<string, AssemblyDefinitionFile> definitions = new Dictionary<string, AssemblyDefinitionFile>();
			foreach (var id in AssetDatabase.FindAssets("t:asmdef", new string[] { folder }))
			{
				definitions.Add(AssetDatabase.GUIDToAssetPath(id), Load(AssetDatabase.GUIDToAssetPath(id)));
			}
			return definitions;
		}
	}
}