using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Interfaces;

namespace WeaverCore.Editor.Utilities
{
	[Serializable]
	public abstract class ConfigSettings : IConfigSettings
	{
		static DirectoryInfo settingsLocation = new DirectoryInfo("Assets\\WeaverCore\\Hidden~");

		static DirectoryInfo GetSettingsLocation()
		{
			return new DirectoryInfo("Assets\\WeaverCore\\Hidden~");
		}

		public virtual string FileName
		{
			get
			{
				return GetType().Name;
			}
		}

		public static C Retrieve<C>() where C : IConfigSettings, new()
		{
			var settings = new C();
			settings.GetStoredSettings();
			return settings;
		}

		public virtual void GetStoredSettings()
		{
			var saveFile = settingsLocation.FullName + "\\" + FileName + ".txt";
			if (File.Exists(saveFile))
			{
				JsonUtility.FromJsonOverwrite(File.ReadAllText(settingsLocation.FullName + "\\" + FileName + ".txt"), this);
			}
		}

		public virtual void SetStoredSettings()
		{
			File.WriteAllText(settingsLocation.FullName + "\\" + FileName + ".txt", JsonUtility.ToJson(this, true));
		}
	}
}
