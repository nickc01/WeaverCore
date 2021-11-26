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
		public static DirectoryInfo DumpLocation
		{
			get
			{
				return new FileInfo(typeof(Initialization).Assembly.Location).Directory.CreateSubdirectory("WeaverCore").CreateSubdirectory("Dumps");//.AddSlash() + "WeaverCore\\Dumps";
			}
		}
	}
}
