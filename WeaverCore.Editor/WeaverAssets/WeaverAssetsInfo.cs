using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WeaverCore.Editor.Internal;

namespace WeaverCore.Editor
{
	public static class WeaverAssetsInfo
	{
		public static bool InWeaverAssetsProject
		{
			get
			{
				return !new FileInfo(WeaverCoreBuilder.DefaultBuildLocation).Directory.Exists;
			}
		}
	}
}
