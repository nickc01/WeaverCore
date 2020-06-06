using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Implementations;

namespace WeaverCore.Helpers
{
	public static class WeaverEvents
	{
		static WeaverEventsImplementation impl = ImplFinder.GetImplementation<WeaverEventsImplementation>();

		public static void BroadcastEvent(string eventName)
		{
			impl.BroadcastEvent(eventName);
		}
	}
}
