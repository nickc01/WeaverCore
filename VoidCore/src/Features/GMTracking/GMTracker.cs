using Modding;
using System;
using System.Collections.Generic;
using UnityEngine;
using VoidCore.Helpers;
using VoidCore.Hooks.Internal;

namespace VoidCore
{

    /// <summary>
    /// Keeps track of all the gameObjects in the scene. 
    /// </summary>
    public class GMTracker : MonoBehaviour
    {
        /// <summary>
        /// Whether a newly spawned gameObject should be tracked or not. Set this to false before spawning a gameObject to prevent that gameObject from getting tracked
        /// </summary>
        internal static bool DoTracking = true;

        /// <summary>
        /// A cache for all inactive trackers.
        /// </summary>
        private static readonly Stack<GMTracker> Cache = new Stack<GMTracker>();

        /// <summary>
        /// A list of all the currently active trackers
        /// </summary>
        private static readonly HashSet<GMTracker> SpawnedTrackers = new HashSet<GMTracker>();

        /// <summary>
        /// A property table that keeps track if a gameObject is being tracked or not
        /// </summary>
        private static readonly PropertyTable<GameObject, GMProps> ObjectPropTable = new PropertyTable<GameObject, GMProps>();

        /// <summary>
        /// A list of the currently loaded hooks on the tracker
        /// </summary>
        private readonly List<GMTrackerHookBase> Hooks = new List<GMTrackerHookBase>();

        /// <summary>
        /// Stores whether the tracked gameObject is active or not
        /// </summary>
        private bool isActive = false;

        /// <summary>
        /// Whether the tracker just started or not
        /// </summary>
        private bool justStarted = true;

        /// <summary>
        /// Whether the tracker is being run for the first time or not. This is used to call the Awake() function
        /// </summary>
        private bool firstRun = false;

        /// <summary>
        /// An event all trackers subscribe to. This is used to stop all of the trackers
        /// </summary>
        private static event Action StopAllEvents;

        /// <summary>
        /// How many active hooks are currently active on this tracker
        /// </summary>
        public int ActiveHooks { get; internal set; } = 0;

        /// <summary>
        /// Whether this tracker is currently active, meaning, it is tracking another gameObject
        /// </summary>
        public bool IsActive => SourceObject != null;

        /// <summary>
        /// The gameObject that this tracker is tracking
        /// </summary>
        public GameObject SourceObject { get; private set; }


        /// <summary>
        /// A list of all the trackers, both active and inactive
        /// </summary>
        internal static IEnumerable<GMTracker> AllTrackers
        {
            get
            {
                foreach (GMTracker cacheTracker in Cache)
                {
                    yield return cacheTracker;
                }
                foreach (GMTracker spawnedWatcher in SpawnedTrackers)
                {
                    yield return spawnedWatcher;
                }
            }
        }

        /// <summary>
        /// Creates a new tracker object to track the specified gameObject
        /// </summary>
        /// <param name="tracker">The specified gameObject to be tracked</param>
        /// <returns>The new tracker</returns>
        public static GMTracker Create(GameObject tracker)
        {
            GMTracker nextTracker = null;
            if (Cache.Count > 0)
            {
                nextTracker = Cache.Pop();
                SpawnedTrackers.Add(nextTracker);
            }
            else
            {
                //Modding.Logger.Log("BUILDING WATCHER");
                GMTracker.DoTracking = false;
                GameObject holder = new GameObject("__GMWATCHERHOLDER__", typeof(GMTracker));
                GameObject.DontDestroyOnLoad(holder);
                GMTracker.DoTracking = true;
                nextTracker = holder.GetComponent<GMTracker>();
                foreach (KeyValuePair<Hooks.Utility.IAllocator, (Type hook, IMod mod)> hookType in GMTrackerAllocator.GMTrackingHooks)
                {
                    nextTracker.AddHook(hookType.Value.hook, hookType.Value.mod);
                }
            }
            nextTracker.gameObject.SetActive(true);
            nextTracker.Begin(tracker);
            //Modding.Logger.Log("END WATCHER");
            return nextTracker;
        }

        /// <summary>
        /// Stops the tracker and caches it for later use
        /// </summary>
        /// <param name="tracker">The tracker to be stopped</param>
        public static void Stop(GMTracker tracker)
        {
            tracker.End();
            tracker.gameObject.SetActive(false);
            SpawnedTrackers.Remove(tracker);
            Cache.Push(tracker);
        }

        /// <summary>
        /// Begins tracking the object using a <see cref="GMTracker"/>
        /// </summary>
        /// <param name="gameObject">The gameObject to track</param>
        /// <returns>Returns whether the object is actually being tracked or not. It may not be tracked because the <see cref="DoTracking"/> may be set to false</returns>
        internal static bool TrackObject(GameObject gameObject)
        {
            GMProps prop = ObjectPropTable.GetOrCreate(gameObject);
            if (!DoTracking)
            {
                prop.ShouldBeTracked = false;
            }
            if (!prop.IsTracked && prop.ShouldBeTracked && DoTracking && Settings.GMTracking)
            {
                prop.IsTracked = true;
                GMTracker.Create(gameObject);
            }
            return prop.ShouldBeTracked;
        }

        /// <summary>
        /// Similar to <see cref="TrackObject(GameObject)"/>, but also tracks the gameObject's children too
        /// </summary>
        /// <param name="gameObject">The gameObject to be tracked. It's children will also be tracked</param>
        internal static void TrackObjectRecursive(GameObject gameObject)
        {
            GMProps props = ObjectPropTable.GetOrCreate(gameObject);
            if (Settings.GMTracking && !props.IsTracked)
            {
                if (TrackObject(gameObject))
                {
                    if (gameObject.transform.childCount > 0)
                    {
                        for (int i = 0; i < gameObject.transform.childCount; i++)
                        {
                            TrackObjectRecursive(gameObject.transform.GetChild(i).gameObject);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clears and destroys all of the trackers
        /// </summary>
        internal static void ClearAll()
        {
            StopAll();
            for (int i = 0; i < Cache.Count; i++)
            {
                GMTracker top = Cache.Pop();
                Destroy(top.gameObject);
            }
        }

        /// <summary>
        /// Whether the gameObject is being tracked or not
        /// </summary>
        /// <param name="gameObject">The gameObject to check</param>
        /// <returns>Returns true or false whether the gameObject is being tracked</returns>
        internal static bool IsBeingTracked(GameObject gameObject)
        {
            return ObjectPropTable.GetOrCreate(gameObject).IsTracked;
        }

        /// <summary>
        /// Stops all of the trackers and returns them to the cache
        /// </summary>
        internal static void StopAll()
        {
            StopAllEvents?.Invoke();
        }

        /// <summary>
        /// Adds a hook to the tracker
        /// </summary>
        /// <param name="hookType">The hook to add</param>
        /// <param name="Mod">The mod the hook came from</param>
        internal void AddHook(Type hookType, IMod Mod)
        {
            GMTrackerHookBase hookInstance = (GMTrackerHookBase)Activator.CreateInstance(hookType);
            hookInstance.Watcher = this;
            IHookBase iHookInstance = (IHookBase)hookInstance;
            iHookInstance.LoadHook(Mod);
            Hooks.Add(hookInstance);
        }

        /// <summary>
        /// Removes a hook from the tracker
        /// </summary>
        /// <param name="hookType">The hook type to remove</param>
        internal void RemoveHook(Type hookType)
        {
            for (int i = Hooks.Count - 1; i >= 0; i--)
            {
                if (Hooks[i].GetType() == hookType)
                {
                    GMTrackerHookBase hook = Hooks[i];
                    ((IHookBase)hook).UnloadHook(hook.currentMod);
                    Hooks.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Starts up the tracker
        /// </summary>
        /// <param name="tracker">The gameObject to be tracked</param>
        private void Begin(GameObject tracker)
        {
            StopAllEvents += OnReturnAll;
            SourceObject = tracker;
            isActive = SourceObject.activeInHierarchy;
            ActiveHooks = Hooks.Count;
            foreach (GMTrackerHookBase hook in Hooks)
            {
                hook.Watcher = this;
                hook.Enabled = true;
            }
            if (!firstRun)
            {
                firstRun = true;
                foreach (GMTrackerHookBase hook in Hooks)
                {
                    hook.Awake();
                }
            }
            if (ActiveHooks == 0)
            {
                return;
            }
            foreach (GMTrackerHookBase hook in Hooks)
            {
                hook.Reset();
            }
            Events.GMCreated.Invoker?.Invoke(SourceObject);
            justStarted = true;
        }

        /// <summary>
        /// Stops the tracker. This is called when the <see cref="Stop(GMTracker)"/> function is called
        /// </summary>
        private void End()
        {
            StopAllEvents -= OnReturnAll;
            SourceObject = null;
            foreach (GMTrackerHookBase hook in Hooks)
            {
                if (hook.Enabled)
                {
                    hook.OnDestroy();
                }
                hook.Watcher = null;
            }
        }


        /// <summary>
        /// Called when the <see cref="StopAllEvents"/> is called. This is used to stop the tracker
        /// </summary>
        private void OnReturnAll()
        {
            Stop(this);
        }

        /// <summary>
        /// Called each frame
        /// </summary>
        private void Update()
        {
            try
            {
                if (!UnityHelpers.IsNativeObjectAlive(SourceObject))
                {
                    Stop(this);
                    return;
                }
                if (IsActive)
                {
                    transform.position = SourceObject.transform.position;
                    if (justStarted)
                    {
                        justStarted = false;
                        foreach (GMTrackerHookBase hook in Hooks)
                        {
                            if (hook.Enabled)
                            {
                                hook.Start();
                            }
                        }
                    }
                    if (SourceObject != null && isActive != SourceObject.activeInHierarchy)
                    {
                        isActive = !isActive;
                        if (isActive)
                        {
                            Events.GMEnabled.Invoker?.Invoke(SourceObject);
                            foreach (GMTrackerHookBase hook in Hooks)
                            {
                                if (hook.Enabled)
                                {
                                    hook.OnEnable();
                                }
                            }
                        }
                        else
                        {
                            Events.GMDisabled.Invoker?.Invoke(SourceObject);
                            foreach (GMTrackerHookBase hook in Hooks)
                            {
                                if (hook.Enabled)
                                {
                                    hook.OnDisable();
                                }
                            }
                        }
                    }
                    if (SourceObject != null)
                    {
                        foreach (GMTrackerHookBase hook in Hooks)
                        {
                            if (hook.Enabled)
                            {
                                hook.Update();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Modding.Logger.Log("Watcher Error = " + e);
            }
        }

        /// <summary>
        /// The properties of a tracked gameObject
        /// </summary>
        private class GMProps
        {
            /// <summary>
            /// Whether the gameObject is being tracked or not
            /// </summary>
            public bool IsTracked = false;

            /// <summary>
            /// Whether the gameObject SHOULD be tracked or not
            /// </summary>
            public bool ShouldBeTracked = true;
        }
    }
}