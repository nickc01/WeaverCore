using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;
using UnityEngine;
using CrystalCore.Hooks.Internal;
using CrystalCore.Hooks.Utility;

namespace CrystalCore.Hooks
{
    public abstract class GMTrackerHook<TMod> : GMTrackerHookBase, IHook<TMod, GMTrackerAllocator> where TMod : IMod
    {
        void IHookBase.LoadHook(IMod mod)
        {
            currentMod = mod;
        }

        void IHookBase.UnloadHook(IMod mod)
        {

        }
    }

    namespace Internal
    {
        public abstract class GMTrackerHookBase
        {
            public bool Enabled { get; internal set; }

            public GMTracker Watcher { get; internal set; }
            public GameObject WatcherObject => Watcher?.gameObject;
            public Transform WatcherTransform => Watcher?.transform;

            public GameObject gameObject => Watcher?.SourceObject;
            public Transform transform => gameObject?.transform;

            internal IMod currentMod = null;

            /// <summary>
            /// Used to reset the object to its base state
            /// </summary>
            public virtual void Reset()
            {

            }

            /// <summary>
            /// Called when the Tracker starts up. This is called only once
            /// </summary>
            public virtual void Awake()
            {

            }

            /// <summary>
            /// Called when the Tracker starts up. This is called whenever this tracker is attached to a new gameObject
            /// </summary>
            public virtual void Start()
            {

            }

            /// <summary>
            /// Called each frame
            /// </summary>
            public virtual void Update()
            {

            }

            /// <summary>
            /// Called when the gameObject is destroyed. NOTE: The gameObject is no longer accessable when this function is called.
            /// </summary>
            public virtual void OnDestroy()
            {

            }

            /// <summary>
            /// Called when the gameObject is enabled
            /// </summary>
            public virtual void OnEnable()
            {

            }

            /// <summary>
            /// Called when the gamebject is disabled
            /// </summary>
            public virtual void OnDisable()
            {
                
            }

            public void StopHook()
            {
                Enabled = false;
                Watcher.ActiveHooks--;
                if (Watcher.ActiveHooks == 0)
                {
                    GMTracker.Stop(Watcher);
                }
            }


            public T GetComponent<T>()
            {
                return gameObject.GetComponent<T>();
            }

            public Component GetComponent(Type type)
            {
                return gameObject.GetComponent(type);
            }

            public T GetComponentInParent<T>()
            {
                return gameObject.GetComponentInParent<T>();
            }

            public Component GetComponentInParent(Type type)
            {
                return gameObject.GetComponentInParent(type);
            }

            public T GetWatcherComponent<T>()
            {
                return Watcher.gameObject.GetComponent<T>();
            }

            public Component GetWatcherComponent(Type type)
            {
                return Watcher.gameObject.GetComponent(type);
            }

            public T AddWatcherComponent<T>() where T : Component
            {
                return Watcher.gameObject.AddComponent<T>();
            }

            public Component AddWatcherComponent(Type type)
            {
                return Watcher.gameObject.AddComponent(type);
            }
        }

        public class GMTrackerAllocator : IAllocator
        {
            internal static Dictionary<IAllocator, (Type hook, IMod mod)> GMTrackingHooks = new Dictionary<IAllocator, (Type, IMod)>();

            IHookBase IAllocator.Allocate(Type hook, IMod mod)
            {
                GMTrackingHooks.Add(this, (hook, mod));
                foreach (var watcher in GMTracker.AllTrackers)
                {
                    watcher.AddHook(hook, mod);
                }
                return null;
            }

            void IAllocator.Deallocate(IHookBase hook, IMod mod)
            {
                var pair = GMTrackingHooks[this];
                GMTrackingHooks.Remove(this);
                foreach (var watcher in GMTracker.AllTrackers)
                {
                    watcher.RemoveHook(pair.hook);
                }
            }
        }
    }
}
