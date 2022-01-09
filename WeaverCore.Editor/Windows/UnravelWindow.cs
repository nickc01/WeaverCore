using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Enums;
using WeaverCore.Interfaces;

namespace WeaverCore.Editor
{
    public class UnravelWindow : EditorWindow
	{
		public static TextureUnravelSettings Settings { get; private set; }

		static bool Open = false;
		static bool Done = false;

		public static IEnumerator GetUnravelSettings()
		{
			if (Open)
			{
				throw new Exception("The Texture Unravel Window is already open");
			}
			Settings = new TextureUnravelSettings();
			Open = true;
			Done = false;
			UnravelWindow window = (UnravelWindow)GetWindow(typeof(UnravelWindow));
			window.Show();

			yield return new WaitUntil(() => Done);
		}

		void OnGUI()
		{
			Settings.texture = (Texture2D)EditorGUILayout.ObjectField("Texture to Unravel", Settings.texture, typeof(Texture2D),false);
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Unpacking mode");
			Settings.UnravelMode = (UnravelMode)EditorGUILayout.EnumPopup(Settings.UnravelMode);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.LabelField("Path to Sheet File");

			EditorGUILayout.BeginHorizontal();

			Settings.SheetPath = EditorGUILayout.TextField(Settings.SheetPath);
			if (GUILayout.Button("...", GUILayout.MaxWidth(30)))
			{
				string sheetLocation = EditorUtility.OpenFilePanel("Find the Sheet File to be used", Path.GetTempPath(), "sheet");
				if (sheetLocation != null && sheetLocation != "")
				{
					Settings.SheetPath = sheetLocation;
				}
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Close"))
			{
				Settings = null;
				Finish();
			}
			if (GUILayout.Button("Unravel") && Open)
			{
				Finish();
			}

			EditorGUILayout.EndHorizontal();
		}

		void Finish(bool close = true)
		{
			if (Open)
			{
				Done = true;
				if (close)
				{
					Close();
				}
				Open = false;
			}
		}


		void OnDestroy()
		{
			if (Open == false)
			{
				Settings = null;
			}
			Finish(false);
		}
	}
}
