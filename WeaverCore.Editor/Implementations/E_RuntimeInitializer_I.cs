using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Helpers;
using WeaverCore.Implementations;
using WeaverCore.Internal;

namespace WeaverCore.Editor.Implementations
{
	public class E_RuntimeInitializer_I : RuntimeInitializer_I
	{
		public override void Awake()
		{
			ModLoader.LoadAllMods();
		}
	}
}
