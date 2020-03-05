using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Helpers;
using WeaverCore.Implementations;
using static WeaverCore.Helpers.Harmony;

namespace WeaverCore.Editor.Implementations
{
	public class EditorEventReceiverImplementation : EventReceiverImplementation
	{
		public override void ReceiveEventsFromObject(GameObject obj, GameObject toSendTo)
		{
			throw new System.NotImplementedException();
		}

		public override void BroadcastEvent(string eventName)
		{
			throw new System.NotImplementedException();
		}

		public override void ReceiveAllEventsOfName(string eventName, GameObject destination)
		{
			throw new System.NotImplementedException();
		}

		public override void Initialize(HarmonyInstance patcher)
		{
			//Nothing Extra Needed
		}

		public override void RemoveReceiver(GameObject obj)
		{
			throw new System.NotImplementedException();
		}

		public override void SendEvent(GameObject toSendTo, string eventName)
		{
			throw new System.NotImplementedException();
		}
	}
}
