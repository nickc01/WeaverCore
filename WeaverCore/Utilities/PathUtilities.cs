using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace WeaverCore.Utilities
{
	public static class PathUtilities
	{

		private readonly static string reservedCharacters = "!*'();:@&=+$,/?%#[]";
		public readonly static DirectoryInfo AssetsFolder = new DirectoryInfo("Assets");
		public readonly static DirectoryInfo ProjectFolder = new DirectoryInfo($"Assets{Path.DirectorySeparatorChar}..");

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
		/// <returns></returns>
		public static string GetFileName(this FileInfo info)
		{
			return info.Name.Split('.')[0];
		}

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

		public static string MakePathRelative(DirectoryInfo relativeTo, string path)
		{
			return MakePathRelative(relativeTo.FullName, path);
		}

		public static IEnumerable<string> MakePathsRelative(string relativeTo, IEnumerable<string> paths)
		{
			foreach (var path in paths)
			{
				yield return MakePathRelative(relativeTo, path);
			}
		}

		public static IEnumerable<string> MakePathsRelative(DirectoryInfo relativeTo, IEnumerable<string> paths)
		{
			return MakePathsRelative(relativeTo, paths);
		}

		public static string ConvertToAssetPath(string path)
		{
			var relative = MakePathRelative(AssetsFolder.FullName, path);
			relative = relative.Replace('\\','/');
			return relative;
		}

		public static string ConvertToProjectPath(string path)
		{
			var relative = MakePathRelative(ProjectFolder.FullName, path);
			relative = relative.Replace('\\', '/');
			return relative;
		}

		public static string AddSlash(string path)
		{
			var slashChar = Path.DirectorySeparatorChar;


			if (!path.EndsWith(slashChar.ToString()))
			{
				return path + slashChar;
			}
			return path;
			/*if (path.Any(c => c == '/'))
			{
				if (!path.EndsWith("/"))
				{
					//Debug.Log("A");
					return path + "/";
				}
			}
			else
			{
				if (!path.EndsWith("\\"))
				{
					//Debug.Log("B");
					return path + "\\";
				}
			}
			//Debug.Log("C");
			return path;*/
		}

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
