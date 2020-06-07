using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Helpers;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
	public class E_RegistryLoader_I : RegistryLoader_I
	{
		public override IEnumerable<Registry> GetRegistries(Type ModType)
		{
			string[] guids = AssetDatabase.FindAssets("t:Registry");
			foreach (var guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);

				ScriptableObject obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
				if (obj is Registry reg)
				{
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
		}
	}
}
