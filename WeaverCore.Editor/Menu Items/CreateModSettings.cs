using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tizen;
using WeaverCore.Configuration;
using WeaverCore.Editor.Utilities;

namespace WeaverCore.Editor
{
	public class CreateModSettings : EditorWindow
	{
		int index = 0;
		List<Type> SettingsTypes = new List<Type>();
		string[] settingsNames;



		[MenuItem("WeaverCore/Create/Mod Settings File")]
		public static void CreateModSettingsMenuItem()
		{
			var window = GetWindow<CreateModSettings>();
			var resolution = Screen.currentResolution;
			var width = window.position.width;
			var height = window.position.height;

			var x = (resolution.width / 2) - (width / 2);
			var y = (resolution.height / 2) - (height / 2);

			window.position = new Rect(x, y, width, height);
			window.titleContent.text = "Mod Settings";

			window.Show();
		}


		void Awake()
		{
			index = 0;
			GetSettingsTypes();
		}

		void GetSettingsTypes()
		{
			List<string> names = new List<string>();
			SettingsTypes.Clear();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					foreach (var type in assembly.GetTypes())
					{
						if (typeof(GlobalWeaverSettings).IsAssignableFrom(type) &&
							!type.IsAbstract &&
							!type.ContainsGenericParameters &&
							!type.IsInterface)
						{
							SettingsTypes.Add(type);
							names.Add(type.Name);
						}
					}
				}
				catch (Exception)
				{
					//Assembly is broken, skip over it
				}
			}
			settingsNames = names.ToArray();
		}




		void OnGUI()
		{
			if (SettingsTypes.Count == 0)
			{
				EditorGUILayout.LabelField("No Mod Settings Types have been found");
				return;
			}
			EditorGUILayout.LabelField("Select the Mod Settings File to Create:");
			index = EditorGUILayout.Popup(index, settingsNames);

			if (GUILayout.Button("Create"))
			{
				Close();
				AssetUtilities.CreateScriptableObject(SettingsTypes[index]);
			}
		}
	}
}
