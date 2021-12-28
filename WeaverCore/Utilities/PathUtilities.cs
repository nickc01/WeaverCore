using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace WeaverCore.Utilities
{
	/// <summary>
	/// Contains utility functions related to file paths
	/// </summary>
	public static class PathUtilities
	{
		/// <summary>
		/// A list of reserved characters that can't exist in a file's name
		/// </summary>
		public readonly static string reservedCharacters = "!*'();:@&=+$,/?%#[]";

		/// <summary>
		/// A path to the "Assets" directory in the Unity Project
		/// </summary>
		public readonly static DirectoryInfo AssetsFolder = new DirectoryInfo("Assets");

		/// <summary>
		/// A path to the Project directory in the Unity Project
		/// </summary>
		public readonly static DirectoryInfo ProjectFolder = new DirectoryInfo($"Assets{Path.DirectorySeparatorChar}..");

		/// <summary>
		/// Makes a global path relative to an existing directory
		/// </summary>
		/// <param name="relativeTo">The existing directory that the new path will be made relative to</param>
		/// <param name="path">The path that will be made relative to the "<paramref name="relativeTo"/>" path</param>
		/// <returns>Returns a new path that is relative to the "<paramref name="relativeTo"/>" directory</returns>
		public static string MakePathRelative(string relativeTo, string path)
		{
			if (relativeTo.Last() != '\\')
			{
				relativeTo += "\\";
			}

			Uri fullPath = new Uri(path, UriKind.Absolute);
			Uri relRoot = new Uri(relativeTo, UriKind.Absolute);

			return RemoveURIEscapeChars(relRoot.MakeRelativeUri(fullPath).ToString());
		}

		/// <summary>
		/// Returns the file name (without the extension)
		/// </summary>
		public static string GetFileName(this FileInfo info)
		{
			return info.Name.Split('.')[0];
		}

		/// <summary>
		/// Converts url escape characters into their actual characters
		/// </summary>
		/// <param name="input">The path to filter</param>
		/// <returns>Returns the path but with the URL Escape Characters removed</returns>
		/// <remarks>
		/// See here for more info on URL Escape Characters : https://docs.microfocus.com/OMi/10.62/Content/OMi/ExtGuide/ExtApps/URL_encoding.htm
		/// </remarks>
		public static string RemoveURIEscapeChars(string input)
		{
			StringBuilder builder = new StringBuilder(input);
			var matches = Regex.Matches(input, @"%([\da-fA-F]{1,2})");
			for (int i = matches.Count - 1; i >= 0; i--)
			{
				builder.Remove(matches[i].Index, matches[i].Length);
				builder.Insert(matches[i].Index, (char)int.Parse(matches[i].Groups[1].Value, System.Globalization.NumberStyles.HexNumber));
			}
			return builder.ToString();
		}

		/// <summary>
		/// Makes a global path relative to an existing directory
		/// </summary>
		/// <param name="relativeTo">The existing directory that the new path will be made relative to</param>
		/// <param name="path">The path that will be made relative to the "<paramref name="relativeTo"/>" path</param>
		/// <returns>Returns a new path that is relative to the "<paramref name="relativeTo"/>" directory</returns>
		public static string MakePathRelative(DirectoryInfo relativeTo, string path)
		{
			return MakePathRelative(relativeTo.FullName, path);
		}

		/// <summary>
		/// Makes a list of paths relative to an existing directory
		/// </summary>
		/// <param name="relativeTo">The existing directory that the new path will be made relative to</param>
		/// <param name="paths">The paths that will be made relative to the "<paramref name="relativeTo"/>" path</param>
		/// <returns>Returns a list of new paths that are relative to the "<paramref name="relativeTo"/>" directory</returns>
		public static IEnumerable<string> MakePathsRelative(string relativeTo, IEnumerable<string> paths)
		{
			foreach (var path in paths)
			{
				yield return MakePathRelative(relativeTo, path);
			}
		}

		/// <summary>
		/// Makes a list of paths relative to an existing directory
		/// </summary>
		/// <param name="relativeTo">The existing directory that the new path will be made relative to</param>
		/// <param name="paths">The paths that will be made relative to the "<paramref name="relativeTo"/>" path</param>
		/// <returns>Returns a list of new paths that are relative to the "<paramref name="relativeTo"/>" directory</returns>
		public static IEnumerable<string> MakePathsRelative(DirectoryInfo relativeTo, IEnumerable<string> paths)
		{
			return MakePathsRelative(relativeTo, paths);
		}

		/// <summary>
		/// Converts an absolute path into a path that is relative to the "Assets" folder
		/// </summary>
		/// <param name="path">The absolute path to be made relative</param>
		/// <returns>Returns the path but relative to the "Assets" folder</returns>
		public static string ConvertToAssetPath(string path)
		{
			var relative = MakePathRelative(AssetsFolder.FullName, path);
			relative = relative.Replace('\\','/');
			return relative;
		}

		/// <summary>
		/// Converts an absolute path into a path that is relative to the Project folder
		/// </summary>
		/// <param name="path">The absolute path to be made relative</param>
		/// <returns>Returns the path but relative to the Project folder</returns>
		public static string ConvertToProjectPath(string path)
		{
			var relative = MakePathRelative(ProjectFolder.FullName, path);
			relative = relative.Replace('\\', '/');
			return relative;
		}

		/// <summary>
		/// Ensures that a slash is added to the end of a path
		/// </summary>
		/// <param name="path">The path to add a slash to</param>
		/// <returns>Returns the path but with a slash at the end</returns>
		public static string AddSlash(string path)
		{
			var slashChar = Path.DirectorySeparatorChar;


			if (!path.EndsWith(slashChar.ToString()))
			{
				return path + slashChar;
			}
			return path;
		}

		/// <summary>
		/// Ensures that a slash is added to the end of a path
		/// </summary>
		/// <param name="path">The path to add a slash to</param>
		/// <returns>Returns the path but with a slash at the end</returns>
		public static string AddSlash(this DirectoryInfo directory)
		{
			return AddSlash(directory.FullName);
		}

		/// <summary>
		/// Replaces all backslashes (\) with forward slashes (/) to make the path work with some unity functions
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string ReplaceSlashes(string path)
		{
			return path.Replace('\\', '/');
		}

	}
}
