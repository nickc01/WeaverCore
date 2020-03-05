using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Helpers;
using static WeaverCore.Helpers.Harmony;

namespace WeaverCore.Implementations
{
    public abstract class EventReceiverImplementation : IImplementation
    {
        public abstract void Initialize(HarmonyInstance patcher);

        public abstract void SendEvent(GameObject destination, string eventName);

        public abstract void BroadcastEvent(string eventName);

        public abstract void ReceiveEventsFromObject(GameObject hijacked, GameObject destination);

        public abstract void ReceiveAllEventsOfName(string eventName, GameObject destination);

        public abstract void RemoveReceiver(GameObject destination);
    }
}
