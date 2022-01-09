using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Implementations;

using System.Collections.Generic;

namespace WeaverCore.Editor.Implementations
{
	public class E_EventManager_I : EventManager_I
	{
		public class E_Statics : Statics
		{
			public override void BroadcastToPlaymakerFSMs(string eventName, GameObject source, bool skipEventManagers)
			{
				//Nothing extra needs to be done in the editor
			}

			public override void TriggerEventToGameObjectPlaymakerFSMs(string eventName, GameObject destination, GameObject source, bool skipEventManagers)
			{
				//Nothing extra needs to be done in the editor
			}
		}
	}
}
