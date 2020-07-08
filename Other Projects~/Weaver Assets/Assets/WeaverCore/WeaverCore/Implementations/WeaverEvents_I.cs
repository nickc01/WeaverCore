using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class WeaverEvents_I : IImplementation
	{
		public abstract void BroadcastEvent(string eventName,GameObject source);
		public abstract void SendEventToObject(GameObject gameObject, string eventName);
	}
}
