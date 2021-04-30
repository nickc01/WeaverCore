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
	//[ShowFeature]
	//public abstract class GlobalWeaverSettings : ScriptableObject, IFeature, IOnFeatureLoad
	//{
		//bool loaded = false;

		//static HashSet<GlobalWeaverSettings> LoadedSettings = new HashSet<GlobalWeaverSettings>();

		/*protected virtual void Awake()
		{
			if (LoadedSettings.Add(this))
			{
				LoadSettings();
			}
		}

		protected virtual void OnEnable()
		{
			if (LoadedSettings.Add(this))
			{
				LoadSettings();
			}
		}

		protected virtual void OnDisable()
		{
			LoadedSettings.Remove(this);
		}

		protected virtual void OnDestroy()
		{
			LoadedSettings.Remove(this);
		}*/




		/*static GlobalWeaverSettings_I impl = ImplFinder.GetImplementation<GlobalWeaverSettings_I>();

		public string TabName;

		public static T GetSettings<T>() where T : GlobalWeaverSettings
		{
			//return (T)LoadedSettings.FirstOrDefault(s => s is T);
			return Registry.GetAllFeatures<T>().FirstOrDefault();
		}

		public static IEnumerable<GlobalWeaverSettings> GetAllSettings()
		{
			return Registry.GetAllFeatures<GlobalWeaverSettings>();
		}

		public void LoadSettings()
		{
			impl.LoadSettings(this);
		}

		public void SaveSettings()
		{
			impl.SaveSettings(this);
		}

		void IOnFeatureLoad.OnFeatureLoad(Registry registry)
		{
			LoadSettings();
		}*/
	//}
}
