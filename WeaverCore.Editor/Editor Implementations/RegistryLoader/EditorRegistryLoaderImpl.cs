using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
	public class EditorRegistryLoaderImplementation : RegistryLoaderImplementation
	{
		public override Registry GetRegistry(Type ModType)
		{
			string[] guids = AssetDatabase.FindAssets("t:Registry");
			foreach (var guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);

				ScriptableObject obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
				if (obj is Registry reg && ModType.IsAssignableFrom(reg.ModType))
				{
					return reg;
				}
			}
			return null;
		}
	}
}
