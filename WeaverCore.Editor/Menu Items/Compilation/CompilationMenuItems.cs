using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace WeaverCore.Editor.Compilation
{
	public static class CompilationMenuItems
	{
		[MenuItem("WeaverCore/Compilation/Mod %F5")]
		public static void CompileMod()
		{
			BuildTools.BuildPartialWeaverCore(BuildTools.WeaverCoreBuildLocation);
			BuildScreen.ShowBuildScreen(false);
		}

		[MenuItem("WeaverCore/Compilation/WeaverCore")]
		public static void CompileWeaverCore()
		{
			BuildTools.BuildPartialWeaverCore(BuildTools.WeaverCoreBuildLocation);
			BuildScreen.ShowBuildScreen(true);
		}
	}
}
