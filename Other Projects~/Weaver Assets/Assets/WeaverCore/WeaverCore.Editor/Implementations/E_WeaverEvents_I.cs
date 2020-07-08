using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Implementations
{
	public class E_WeaverEvents_I : WeaverEvents_I
	{
		public override void BroadcastEvent(string eventName,GameObject source)
		{
			foreach (var receiver in EventReceiver.AllReceivers)
			{
				receiver.ReceiveEvent(eventName,source);
			}
		}

		public override void SendEventToObject(GameObject gameObject, string eventName)
		{
			EventReceiver receiver = null;
			if ((receiver = gameObject.GetComponent<EventReceiver>()) != null)
			{
				receiver.ReceiveEvent(eventName,gameObject);
			}
		}
	}
}
