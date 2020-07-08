using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WeaverCore.Utilities
{
	public static class PathUtilities
	{
		public static string MakePathRelative(string relativeTo, string path)
		{
			if (relativeTo.Last() != '\\')
			{
				relativeTo += "\\";
			}

			Uri fullPath = new Uri(path, UriKind.Absolute);
			Uri relRoot = new Uri(relativeTo, UriKind.Absolute);

			return relRoot.MakeRelativeUri(fullPath).ToString();
		}

		public static string AddSlash(string path)
		{
			if (path.Any(c => c == '/'))
			{
				if (!path.EndsWith("/"))
				{
					return path + "/";
				}
			}
			else
			{
				if (!path.EndsWith("\\"))
				{
					return path + "\\";
				}
			}
			return path;
		}

		public static string AddSlash(this DirectoryInfo directory)
		{
			return AddSlash(directory.FullName);
		}
	}
}
