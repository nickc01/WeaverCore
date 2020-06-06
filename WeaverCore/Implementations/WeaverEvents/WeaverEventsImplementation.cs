using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class WeaverEventsImplementation : IImplementation
	{
		public abstract void BroadcastEvent(string eventName);
	}
}
