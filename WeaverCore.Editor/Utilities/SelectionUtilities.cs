using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Compilation;

namespace WeaverCore.Editor.Utilities
{
	[Serializable]
	class LastDirectorySettings
	{
		public string LastSelectedDirectory = Path.GetTempPath();
	}

	public static class SelectionUtilities
	{
		public static string SelectFile(string title,string extension)
		{
			var lastDirectory = GetLastDirectory();
			var file = EditorUtility.OpenFilePanel(title, lastDirectory.LastSelectedDirectory, extension);
			lastDirectory.LastSelectedDirectory = new FileInfo(file).Directory.FullName;
			SetLastDirectory(lastDirectory);
			if (file == null || file == "")
			{
				return null;
			}
			return new FileInfo(file).FullName;
		}

		public static string SelectFolder(string title, string defaultName)
		{
			var lastDirectory = GetLastDirectory();
			var folder = EditorUtility.OpenFolderPanel(title, lastDirectory.LastSelectedDirectory, defaultName);
			lastDirectory.LastSelectedDirectory = folder;
			SetLastDirectory(lastDirectory);
			if (folder == null || folder == "")
			{
				return null;
			}
			return new DirectoryInfo(folder).FullName;
		}



		static LastDirectorySettings GetLastDirectory()
		{
			if (PersistentData.TryGetData(out LastDirectorySettings settings))
			{
				return settings;
			}
			else
			{
				return new LastDirectorySettings();
			}
		}

		static void SetLastDirectory(LastDirectorySettings settings)
		{
			PersistentData.StoreData(settings);
			PersistentData.SaveData();
			//var selections = new LastDirectorySettings();
			//selections.GetStoredSettings();
			//var selections = LastDirectorySettings.Retrieve<LastDirectorySettings>();

			//bool foundExisting = false;

			/*foreach (var selection in selections.LastSelections)
			{
				if (selection.hash == hash && selection.directory != directory)
				{
					selection.directory = directory;
					foundExisting = true;
					//Debug.Log("Last Directory Json = " + JsonUtility.ToJson(selections));
					//selections.SetStoredSettings();
					break;
				}
			}*/

			/*if (!foundExisting)
			{
				selections.LastSelections.Add(new DirectoryPair() { directory = directory, hash = hash });
			}

			selections.SetStoredSettings();*/
		}
	}
}
