using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Internal
{
	public static class Initializer
	{
		public static void Initialize()
		{
			foreach (var checker in RegistryChecker.LoadAllRegistries())
			{
				checker.Check();
			}
		}
	}
}
