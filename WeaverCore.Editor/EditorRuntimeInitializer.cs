using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Implementations;
using WeaverCore.Internal;
using WeaverCore.Interfaces;

namespace WeaverCore.Editor.Implementations
{
	public class EditorRuntimeInitializer : IRuntimeInit
	{
		void IRuntimeInit.RuntimeInit()
		{
			ModLoader.LoadAllMods();
		}
	}
}
