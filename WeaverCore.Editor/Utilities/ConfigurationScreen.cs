using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Interfaces;

namespace WeaverCore.Editor.Utilities
{
	public abstract class ConfigurationScreen<Settings> : EditorWindow where Settings : IConfigSettings, new()
	{
		public static Settings StoredSettings { get; private set; }

		public static void Open(Type windowToOpen)
		{
			ConfigurationScreen<Settings> window = (ConfigurationScreen<Settings>)GetWindow(windowToOpen);
			var resolution = Screen.currentResolution;
			var width = window.position.width;
			var height = window.position.height;

			var x = (resolution.width / 2) - (width / 2);
			var y = (resolution.height / 2) - (height / 2);

			window.position = new Rect(x, y, width, height);
		}

		public static void Open()
		{
			foreach (var type in typeof(IConfigSettings).Assembly.GetTypes())
			{
				if (typeof(ConfigurationScreen<Settings>).IsAssignableFrom(type))
				{
					Open(type);
				}
			}
		}

		protected virtual void Awake()
		{
			StoredSettings = new Settings();
			StoredSettings.GetStoredSettings();
		}


		protected virtual void Done()
		{
			StoredSettings.SetStoredSettings();
			Close();
		}


		protected abstract void OnGUI();
	}
}
