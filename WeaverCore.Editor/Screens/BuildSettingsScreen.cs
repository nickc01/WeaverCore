using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Systems;
using WeaverCore.Editor.Utilities;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;
//using WeaverCore.Editor.Helpers;

namespace WeaverCore.Editor
{
	public class BuildSettingsScreen : ConfigurationScreen<BuildSettings>
	{
		static bool weaverCoreOnly = false;

		public static void BeginModCompile()
		{
			weaverCoreOnly = false;
			Open();
		}

		public static void BeginWeaverCoreCompile()
		{
			weaverCoreOnly = true;
			Open();
		}

		protected override void OnGUI()
		{
			if (!weaverCoreOnly)
			{
				EditorGUILayout.LabelField("Mod Name");
				StoredSettings.ModName = EditorGUILayout.TextField(StoredSettings.ModName);
				EditorGUILayout.Space();
			}

			EditorGUILayout.LabelField("Windows Support");
			StoredSettings.WindowsSupport = EditorGUILayout.Toggle(StoredSettings.WindowsSupport);

			EditorGUILayout.LabelField("Mac Support");
			StoredSettings.MacSupport = EditorGUILayout.Toggle(StoredSettings.MacSupport);

			EditorGUILayout.LabelField("Linux Support");
			StoredSettings.LinuxSupport = EditorGUILayout.Toggle(StoredSettings.LinuxSupport);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Auto-Start Game");
			StoredSettings.StartGame = EditorGUILayout.Toggle(StoredSettings.StartGame);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Build Location");
			EditorGUILayout.BeginHorizontal();
			StoredSettings.BuildLocation = EditorGUILayout.TextField(StoredSettings.BuildLocation);
			if (GUILayout.Button("...", GUILayout.MaxWidth(30)))
			{
				string buildLocation = SelectionUtilities.SelectFolder("Select where the mod should be built", Environment.CurrentDirectory);
				if (buildLocation != null && buildLocation != "")
				{
					var directoryInfo = new DirectoryInfo(buildLocation);

					StoredSettings.BuildLocation = directoryInfo.FullName;
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Close"))
			{
				Close();
			}
			if (GUILayout.Button("Compile"))
			{
				Done();
			}

			EditorGUILayout.EndHorizontal();
		}

		protected override void Done()
		{
			base.Done();
			if (weaverCoreOnly)
			{
				ModCompiler.BuildWeaverCore();
			}
			else
			{
				ModCompiler.BuildMod();
			}
		}
	}
}
