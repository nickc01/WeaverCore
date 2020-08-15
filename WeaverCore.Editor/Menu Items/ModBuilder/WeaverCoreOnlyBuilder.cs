using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Internal;
using WeaverCore.Editor.Systems;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{
	public static class WeaverCoreOnlyBuilder
	{
		[MenuItem("WeaverCore/Compile WeaverCore")]
		public static void Compile()
		{
			BeginCompile();

			Debug.Log("<b>WeaverCore Compile Done</b>");
		}

		static void BeginCompile()
		{
			var settings = new Settings();
			settings.GetStoredSettings();
			//WeaverCoreBuilder.BuildFinishedVersion(settings.HollowKnightDirectory + "\\hollow_knight_data\\Managed\\Mods\\WeaverCore.dll");
			LibraryCompiler.BuildWeaverCore(settings.HollowKnightDirectory + "\\hollow_knight_data\\Managed\\Mods\\WeaverCore.dll");
		}
	}
}
