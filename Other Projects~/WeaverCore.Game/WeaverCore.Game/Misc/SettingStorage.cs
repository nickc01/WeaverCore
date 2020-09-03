
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Configuration;

namespace WeaverCore.Game
{
	public static class SettingStorage
	{
		public static DirectoryInfo SettingsFolder { get; private set; }


		static SettingStorage()
		{
			var weaverCoreFile = new FileInfo(typeof(WeaverCore.CoreInfo).Assembly.Location);
			SettingsFolder = new DirectoryInfo(weaverCoreFile.Directory.FullName + "\\WeaverCore\\Settings");

			SettingsFolder.Create();
		}

		public static void Load<T>(T settings) where T : ModSettings
		{
			Load(typeof(T), settings);
		}

		public static void Load(Type type, ModSettings settings)
		{
			if (!typeof(ModSettings).IsAssignableFrom(type))
			{
				throw new Exception("The type " + type.FullName + " does not inherit from ModSettings");
			}
			var file = new FileInfo(SettingsFolder.FullName + "\\" + type.FullName + ".cfg");

			if (file.Exists)
			{
				try
				{
					JsonUtility.FromJsonOverwrite(File.ReadAllText(file.FullName), settings);
				}
				catch (Exception)
				{
					
				}
			}
		}

		public static void Save(ModSettings settings)
		{
			WeaverLog.Log("Saving");
			var type = settings.GetType();
			var file = new FileInfo(SettingsFolder.FullName + "\\" + type.FullName + ".cfg");
			File.WriteAllText(file.FullName, JsonUtility.ToJson(settings));
		}

	}
}
