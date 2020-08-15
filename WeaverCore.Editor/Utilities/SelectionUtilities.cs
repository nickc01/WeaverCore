using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace WeaverCore.Editor.Utilities
{
	[Serializable]
	class LastDirectorySettings : ConfigSettings
	{
		public List<DirectoryPair> LastSelections = new List<DirectoryPair>();
	}


	[Serializable]
	class DirectoryPair
	{
		public int hash;
		public string directory;
	}



	public static class SelectionUtilities
	{
		public static string SelectFile(string title,string extension)
		{
			var lastDirectory = GetLastDirectory(title.GetHashCode());
			var file = EditorUtility.OpenFilePanel(title, lastDirectory, extension);
			SetLastDirectory(title.GetHashCode(), new FileInfo(file).Directory.FullName);
			if (file == null || file == "")
			{
				return null;
			}
			return new FileInfo(file).FullName;
		}

		public static string SelectFolder(string title, string defaultName)
		{
			var lastDirectory = GetLastDirectory(title.GetHashCode());
			var folder = EditorUtility.OpenFolderPanel(title, lastDirectory, defaultName);
			SetLastDirectory(title.GetHashCode(), folder);
			if (folder == null || folder == "")
			{
				return null;
			}
			return new DirectoryInfo(folder).FullName;
		}



		static string GetLastDirectory(int hash)
		{
			var selections = LastDirectorySettings.Retrieve<LastDirectorySettings>();
			foreach (var selection in selections.LastSelections)
			{
				if (selection.hash == hash)
				{
					return selection.directory;
				}
			}
			return "";
		}

		static void SetLastDirectory(int hash, string directory)
		{
			//var selections = new LastDirectorySettings();
			//selections.GetStoredSettings();
			var selections = LastDirectorySettings.Retrieve<LastDirectorySettings>();

			bool foundExisting = false;

			foreach (var selection in selections.LastSelections)
			{
				if (selection.hash == hash && selection.directory != directory)
				{
					selection.directory = directory;
					foundExisting = true;
					//Debug.Log("Last Directory Json = " + JsonUtility.ToJson(selections));
					//selections.SetStoredSettings();
					break;
				}
			}

			if (!foundExisting)
			{
				selections.LastSelections.Add(new DirectoryPair() { directory = directory, hash = hash });
			}

			selections.SetStoredSettings();
		}
	}
}
