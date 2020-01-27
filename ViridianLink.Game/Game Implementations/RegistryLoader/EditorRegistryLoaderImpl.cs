using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ViridianLink.Core;
using ViridianLink.Implementations;

namespace ViridianLink.Game.Implementations
{
	public class EditorRegistryLoaderImplementation : RegistryLoaderImplementation
	{
		public override Registry GetRegistry(Type ModType)
		{
			//TODO
			return null;
			/*string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
			foreach (var guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);

				ScriptableObject obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
				if (obj is Registry reg && ModType.IsAssignableFrom(reg.ModType))
				{
					return reg;
				}
			}
			return null;*/
		}
	}
}
