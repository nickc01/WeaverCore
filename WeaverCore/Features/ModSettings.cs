using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Configuration
{
	[ShowFeature]
	public abstract class ModSettings : ScriptableObject, IFeature, IOnRegistryLoad
	{
		static List<ModSettings> allSettings = new List<ModSettings>();
		List<Registry> registries = new List<Registry>();


		static ModSettings_I impl = ImplFinder.GetImplementation<ModSettings_I>();

		[SerializeField]
		bool featureEnabled = true;
		public string TabName;

		bool IFeature.FeatureEnabled
		{
			get
			{
				return featureEnabled;
			}
		}

		public static IEnumerable<ModSettings> GetSettingsForMod(WeaverMod mod)
		{
			foreach (var settings in allSettings)
			{
				foreach (var registry in settings.registries)
				{
					if (registry.ModType == mod.GetType())
					{
						yield return settings;
						break;
					}
				}
			}
		}

		public static T GetSettings<T>() where T : ModSettings
		{
			return (T)allSettings.FirstOrDefault(s => s.GetType() == typeof(T));
		}

		public static IEnumerable<ModSettings> GetAllSettings()
		{
			return allSettings;
		}

		public void LoadSettings()
		{
			WeaverLog.Log("Loading Settings for " + GetType().FullName);
			impl.LoadSettings(this);
		}

		public void SaveSettings()
		{
			WeaverLog.Log("Saving Settings for " + GetType().FullName);
			impl.SaveSettings(this);
		}

		void IOnRegistryLoad.OnRegistryLoad(Registry registry)
		{
			allSettings.Add(this);
			registries.Add(registry);
			LoadSettings();
		}
	}
}
