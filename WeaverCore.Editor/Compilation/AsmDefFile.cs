using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		public enum Platform
        {
			Android,
			CloudRendering,
			Editor,
			GameCoreScarlett,
			GameCoreXboxOne,
			iOS,
			LinuxStandalone64,
			Lumin,
			macOSStandalone,
			PS4,
			Stadia,
			Switch,
			tvOS,
			WSA,
			WebGL,
			WindowsStandalone32,
			WindowsStandalone64,
			XboxOne
		}

		public string name;
		public string rootNamespace = "";
		public System.Collections.Generic.List<string> references;

		[SerializeField]
        System.Collections.Generic.List<string> includePlatforms;

		[SerializeField]
        System.Collections.Generic.List<string> excludePlatforms;

		[NonSerialized]
        System.Collections.Generic.List<Platform> _includePlatforms;

        [NonSerialized]
        System.Collections.Generic.List<Platform> _excludePlatforms;

        public System.Collections.Generic.List<Platform> IncludePlatforms
		{
			get => _includePlatforms ??= (includePlatforms ??= new List<string>()).Select(platform => (Platform)Enum.Parse(typeof(Platform), platform)).ToList();
			set
			{
				_includePlatforms = value;
				if (includePlatforms == null)
				{
                    includePlatforms = new System.Collections.Generic.List<string>();
				}
				else
				{
					includePlatforms.Clear();
				}
				includePlatforms.AddRange(value.Select(p => p.ToString()));
			}
		}

        public System.Collections.Generic.List<Platform> ExcludePlatforms
        {
            get => _excludePlatforms ??= (excludePlatforms ??= new List<string>()).Select(platform => (Platform)Enum.Parse(typeof(Platform), platform)).ToList();
            set
            {
                _excludePlatforms = value;
                if (excludePlatforms == null)
                {
                    excludePlatforms = new System.Collections.Generic.List<string>();
                }
                else
                {
                    excludePlatforms.Clear();
                }
                excludePlatforms.AddRange(value.Select(p => p.ToString()));
            }
        }

		public bool IsPlatformSupported(Platform platform)
		{
			if (IncludePlatforms.Count > 0)
			{
				return IncludePlatforms.Contains(platform);
			}
			else
			{
				return !ExcludePlatforms.Contains(platform);
			}
		}

		public bool AnyPlatformsSupported(IEnumerable<Platform> platforms)
		{
			return platforms.Any(IsPlatformSupported);
		}

        public bool allowUnsafeCode = false;
		public bool overrideReferences = false;
		public bool autoReferenced = true;
		public System.Collections.Generic.List<string> defineConstraints;
		public bool noEngineReferences = false;

		[SerializeField]
        System.Collections.Generic.List<string> precompiledReferences;

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