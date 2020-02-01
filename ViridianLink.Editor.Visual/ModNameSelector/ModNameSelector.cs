using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ViridianLink.Editor.Visual
{
	public class ModNameSelector : EditorWindow
	{
		Action<string> OnStart;
		string modName = "";


		string StoredModName
		{
			get
			{
				if (File.Exists("LastModName.dat"))
				{
					using (var file = File.Open("LastModName.dat", FileMode.Open))
					{
						using (var reader = new StreamReader(file))
						{
							return reader.ReadToEnd();
						}
					}
				}
				return "";
			}
			set
			{
				using (var file = File.Open("LastModName.dat", FileMode.Create))
				{
					using (var writer = new StreamWriter(file))
					{
						writer.Write(value);
					}
				}
			}
		}

		public static void ChooseString(Action<string> OnSelection)
		{
			ModNameSelector window = (ModNameSelector)GetWindow(typeof(ModNameSelector));
			window.Setup(OnSelection);
			window.Show();
		}

		void Setup(Action<string> OnSelection)
		{
			var resolution = Screen.currentResolution;
			var width = position.width;
			var height = position.height;

			var x = (resolution.width / 2) - (width / 2);
			var y = (resolution.height / 2) - (height / 2);

			position = new Rect(x, y, width, height);
			titleContent.text = "Compile";

			modName = StoredModName;
			if (modName == "")
			{
				modName = PlayerSettings.productName.Replace(" ", "");
			}

			OnStart = OnSelection;
		}

		void OnGUI()
		{
			EditorGUILayout.LabelField("Specify the name of the mod:");
			modName = EditorGUILayout.TextField(modName);
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Close"))
			{
				Close();
			}
			if (GUILayout.Button("Compile") && OnStart != null)
			{
				Close();
				StoredModName = modName;
				OnStart(modName);
				OnStart = null;
			}

			EditorGUILayout.EndHorizontal();
		}
	}
}
