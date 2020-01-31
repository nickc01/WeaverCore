using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ViridianLink.Editor.Visual
{
	public class ModNameSelector : EditorWindow
	{
		string modNameSelection;
		Action<string> OnStart;


		public static void ChooseString(Action<string> OnSelection)
		{
			ModNameSelector window = (ModNameSelector)GetWindow(typeof(ModNameSelector));

			var resolution = Screen.currentResolution;
			var width = window.position.width;
			var height = window.position.height;

			var x = (resolution.width / 2) - (width / 2);
			var y = (resolution.height / 2) - (height / 2);

			window.position = new Rect(x, y, width, height);
			window.titleContent.text = "Compile";

			var defaultName = "";

			var guids = AssetDatabase.FindAssets("ModName");

			if (guids != null && guids.GetLength(0) > 0)
			{
				var path = AssetDatabase.GUIDToAssetPath(guids[0]);
				TextAsset file = (TextAsset)AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
				defaultName = Regex.Match(file.text, @"""name"": ""(.+?)""").Groups[1].Value;
			}

			if (defaultName == "")
			{
				defaultName = PlayerSettings.productName.Replace(" ", "");
			}
			window.modNameSelection = defaultName;
			window.OnStart = OnSelection;



			window.Show();
		}

		void OnGUI()
		{
			EditorGUILayout.LabelField("Specify the name of the mod:");
			modNameSelection = EditorGUILayout.TextField(modNameSelection);
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Close"))
			{
				Close();
			}
			if (GUILayout.Button("Compile") && OnStart != null)
			{
				Close();
				OnStart(modNameSelection);
				OnStart = null;
			}

			EditorGUILayout.EndHorizontal();
		}
	}
}
