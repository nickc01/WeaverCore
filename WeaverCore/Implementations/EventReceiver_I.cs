using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Interfaces;
using static WeaverCore.Utilities.Harmony;

namespace WeaverCore.Implementations
{
	public abstract class EventReceiver_I : IImplementation
    {
        public abstract void Initialize(HarmonyInstance patcher);

        public abstract void ReceiveEventsFromObject(int instanceID, GameObject destination);

        public abstract void ReceiveAllEventsOfName(string eventName, GameObject destination);

        public abstract void RemoveReceiver(GameObject destination);

        public abstract void OnReceiveEvent(EventReceiver receiver, string eventName);
    }
}
