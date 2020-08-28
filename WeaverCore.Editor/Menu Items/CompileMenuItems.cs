using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverBuildTools.Commands;
using WeaverCore.Editor.Internal;
using WeaverCore.Editor.Systems;
using WeaverCore.Editor.Utilities;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{
	public static class CompileMenuItems
	{
		[MenuItem("WeaverCore/Compile/Mod %F5")]
		public static void CompileMod()
		{
			BuildSettingsScreen.BeginModCompile();
		}

		[MenuItem("WeaverCore/Compile/WeaverCore")]
		public static void CompileWeaverCore()
		{
			BuildSettingsScreen.BeginWeaverCoreCompile();
		}
	}
}
