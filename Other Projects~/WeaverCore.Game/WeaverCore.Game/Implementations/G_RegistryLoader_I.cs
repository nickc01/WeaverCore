using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
	public class G_RegistryLoader_I : RegistryLoader_I
	{
		public override void LoadRegistries(Assembly assembly)
		{
			RegistryLoader.LoadEmbeddedRegistries(assembly);
		}
	}
}
