using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverBuildTools.Utilities
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
	}
}
