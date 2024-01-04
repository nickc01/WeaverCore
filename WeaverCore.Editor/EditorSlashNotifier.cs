using Modding;
using System;
using System.Reflection;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{
    public class EditorSlashNotifier : MonoBehaviour
	{
        static Action<Collider2D, GameObject> onSlashHit;

        private void Awake()
        {
            if (onSlashHit == null)
            {
                onSlashHit = ReflectionUtilities.MethodToDelegate<Action<Collider2D, GameObject>>(typeof(ModHooks).GetMethod("OnSlashHit", BindingFlags.Static | BindingFlags.NonPublic));
            }
        }

        private void OnTriggerEnter2D(Collider2D otherCollider)
        {
            onSlashHit(otherCollider, gameObject);
        }
    }
}
