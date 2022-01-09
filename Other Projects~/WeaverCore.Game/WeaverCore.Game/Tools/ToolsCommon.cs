using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Tools
{
	public static class ToolsCommon
	{
		/// <summary>
		/// The location where all the sprites and animations should be dumped to
		/// </summary>
		public static DirectoryInfo DumpLocation
		{
			get
			{
				return new FileInfo(typeof(Initialization).Assembly.Location).Directory.CreateSubdirectory("WeaverCore").CreateSubdirectory("Dumps");
			}
		}
	}
}
