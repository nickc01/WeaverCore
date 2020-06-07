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
	public class E_EventReceiver_I : EventReceiver_I
	{
		public override void ReceiveEventsFromObject(GameObject obj, GameObject toSendTo)
		{
			
		}

		public override void BroadcastEvent(string eventName)
		{
			
		}

		public override void ReceiveAllEventsOfName(string eventName, GameObject destination)
		{
			
		}

		public override void Initialize(HarmonyInstance patcher)
		{
			//Nothing Extra Needed
		}

		public override void RemoveReceiver(GameObject obj)
		{
			
		}

		public override void SendEvent(GameObject toSendTo, string eventName)
		{
			
		}
	}
}
