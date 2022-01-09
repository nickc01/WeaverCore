using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Utilities;
using WeaverCore.Settings;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{
	public class CreateScriptableObjectWindow : EditorWindow
	{
		int index = 0;
		List<Type> SettingsTypes = new List<Type>();
		string[] settingsNames;

		Type typeToCreate;
		string formalTypeName;
		string errorMessage;

		public static void OpenCreationMenu(string title, Type type, string errorMessage, string formalTypeName = null)
		{
			if (formalTypeName == null)
			{
				formalTypeName = StringUtilities.Prettify(type.Name);
			}
			var window = GetWindow<CreateScriptableObjectWindow>();
			var resolution = Screen.currentResolution;
			var width = window.position.width;
			var height = window.position.height;

			window.typeToCreate = type;
			window.formalTypeName = formalTypeName;
			window.errorMessage = errorMessage;

			var x = (resolution.width / 2) - (width / 2);
			var y = (resolution.height / 2) - (height / 2);

			window.position = new Rect(x, y, width, height);
			window.titleContent.text = title;

			window.GetValidTypes();
			window.Show();
		}

		void Awake()
		{
			index = 0;
		}

		void GetValidTypes()
		{
			List<string> names = new List<string>();
			SettingsTypes.Clear();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					foreach (var type in assembly.GetTypes())
					{
						if (typeToCreate.IsAssignableFrom(type) &&
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
				var messages = errorMessage.Split('\n');
				foreach (var msg in messages)
				{
					EditorGUILayout.LabelField(msg);
				}
				return;
			}
			EditorGUILayout.LabelField("Select the Class to Create:");
			index = EditorGUILayout.Popup(index, settingsNames);

			if (GUILayout.Button("Create"))
			{
				Close();
				AssetUtilities.CreateScriptableObject(SettingsTypes[index]);
			}
		}
	}
}
