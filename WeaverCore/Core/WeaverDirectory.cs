using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WeaverCore.Helpers;
using WeaverCore.Internal;

namespace WeaverCore
{
	public static class WeaverDirectory
	{
		public static DirectoryInfo GetWeaverDirectory()
		{
			var dir = new DirectoryInfo("WeaverCore");

			if (ImplFinder.State == RunningState.Game)
			{
				dir = new DirectoryInfo(TrailingSlash(ModLoader.GetModFolderPath()) + "WeaverCore");
			}

			if (!dir.Exists)
			{
				dir.Create();
			}
			return dir;
		}

		public static string TrailingSlash(string input)
		{
			if (input.Any(c => c == '/'))
			{
				if (!input.EndsWith("/"))
				{
					return input + "/";
				}
			}
			else
			{
				if (!input.EndsWith("\\"))
				{
					return input + "\\";
				}
			}
			return input;
		}

		public static string PathWithSlash(this DirectoryInfo directory)
		{
			return TrailingSlash(directory.FullName);
		}
	}
}
