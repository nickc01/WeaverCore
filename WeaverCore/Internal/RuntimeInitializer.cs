using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Helpers;
using WeaverCore.Implementations;

namespace WeaverCore.Internal
{
	static class RuntimeInitializer
	{
		[RuntimeInitializeOnLoadMethod]
		static void Load()
		{
			var impl = ImplFinder.GetImplementation<RuntimeInitializer_I>();
			impl.Awake();
		}
	}
}
