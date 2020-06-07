using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
	public class E_WeaverEvents_I : WeaverEvents_I
	{
		public override void BroadcastEvent(string eventName)
		{
			//TODO - FIll in
			Debugger.Log("Broadcasting event = " + eventName);
		}
	}
}
