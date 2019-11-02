using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VoidCore.Debug
{
    internal class VoidMonitor : MonoBehaviour
    {
        bool IsReady = false;

        void Start()
        {
            if (gameObject.GetComponents<VoidMonitor>().GetLength(0) > 1)
            {
                Destroy(this);
            }
            else
            {
                IsReady = true;
                Events.InternalGameObjectCreated?.Invoke(gameObject);
            }
        }

        void OnDestroy()
        {
            if (!Application.isEditor && IsReady)
            {
                GameObjectTracker.AllGameObjects.Remove(gameObject);
            }
        }
    }
}
