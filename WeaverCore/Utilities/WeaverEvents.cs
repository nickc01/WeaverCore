using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Implementations;

namespace WeaverCore.Utilities
{
	public static class WeaverEvents
	{
		static WeaverEvents_I impl = ImplFinder.GetImplementation<WeaverEvents_I>();

		public static void BroadcastEvent(string eventName)
		{
			impl.BroadcastEvent(eventName);
		}
	}
}
