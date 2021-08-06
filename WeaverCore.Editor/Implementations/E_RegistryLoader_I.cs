using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
	public class E_RegistryLoader_I : RegistryLoader_I
	{
		/*public override IEnumerable<Registry> GetRegistries(Type ModType)
		{
			string[] guids = AssetDatabase.FindAssets("t:Registry");
			foreach (var guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);

				ScriptableObject obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
				if (obj is Registry)
				{
					var reg = obj as Registry;
					if (ModType.IsAssignableFrom(reg.ModType))
					{
						yield return reg;
					}
				}
			}

			foreach (var registry in RegistryLoader.GetEmbeddedRegistries(ModType))
			{
				yield return registry;
			}
		}*/

		public override void LoadRegistries(Assembly assembly)
		{
			var assemblyName = assembly.GetName().Name;
			string[] guids = AssetDatabase.FindAssets($"t:{nameof(Registry)}");
			foreach (var guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);

				Registry registry = AssetDatabase.LoadAssetAtPath<Registry>(path);
				if (registry.ModAssemblyName == assemblyName)
				{
					registry.EnableRegistry();
				}
			}

			RegistryLoader.LoadEmbeddedRegistries(assembly);
		}
	}
}
