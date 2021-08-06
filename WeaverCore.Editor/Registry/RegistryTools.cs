using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{
	public static class RegistryTools
	{
		public static List<Registry> GetAllRegistries()
		{
			List<Registry> registries = new List<Registry>();
			var guids = AssetDatabase.FindAssets($"t:{nameof(Registry)}");
			foreach (var guid in guids)
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				registries.Add(AssetDatabase.LoadAssetAtPath<Registry>(path));
			}
			return registries;
		}

		static List<Type> modsCached;

		public static List<Type> GetAllMods()
		{
			if (modsCached == null)
			{
				modsCached = new List<Type>();
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					foreach (var type in assembly.GetTypes())
					{
						if (typeof(WeaverMod).IsAssignableFrom(type) && !type.IsGenericType && !type.IsAbstract)
						{
							modsCached.Add(type);
						}
					}
				}
			}
			return modsCached;
		}

		static string[] modNamesCached;

		public static string[] GetAllModNames()
		{
			if (modNamesCached == null)
			{
				var mods = GetAllMods();
				modNamesCached = new string[mods.Count];
				for (int i = 0; i < mods.Count; i++)
				{
					modNamesCached[i] = StringUtilities.Prettify(mods[i].Name);
				}
			}
			
			return modNamesCached;
		}

		static List<Type> featuresCached;

		public static List<Type> GetAllFeatures()
		{
			if (featuresCached == null)
			{
				featuresCached = new List<Type>();
				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					foreach (var type in assembly.GetTypes())
					{
						if (type.IsDefined(typeof(ShowFeatureAttribute), false))
						{
							featuresCached.Add(type);
						}
					}
				}
			}
			
			return featuresCached;
		}

		static string[] featureNamesCached;

		public static string[] GetAllFeatureNames()
		{
			if (featureNamesCached == null)
			{
				var features = GetAllFeatures();
				featureNamesCached = new string[features.Count];
				for (int i = 0; i < features.Count; i++)
				{
					featureNamesCached[i] = StringUtilities.Prettify(features[i].Name);
				}
			}
			return featureNamesCached;
		}
	}
}
