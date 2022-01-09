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

	/// <summary>
	/// Contains several utility functions related to selecting files or directories
	/// </summary>
	public static class SelectionUtilities
	{
		/// <summary>
		/// Prompts the user to select a file
		/// </summary>
		/// <param name="title">The title of the window</param>
		/// <param name="extension">The extension to look for</param>
		/// <returns>Returns the path of the selected file, or null if a file wasn't selected</returns>
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

		/// <summary>
		/// Prompts the user to select a folder
		/// </summary>
		/// <param name="title">The title of the window</param>
		/// <param name="defaultFolder">The default folder to start the window up on</param>
		/// <returns>Returns the path to the selected folder, or null if a folder wasn't selected</returns>
		public static string SelectFolder(string title, string defaultFolder)
		{
			var lastDirectory = GetLastDirectory();
			var folder = EditorUtility.OpenFolderPanel(title, lastDirectory.LastSelectedDirectory, defaultFolder);
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
		}
	}
}
