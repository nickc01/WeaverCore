using Harmony;
using System.Reflection;
using UnityEngine;

namespace ViridianCore
{
    internal static class GMTrackingPatcher
    {
        [ModStart(typeof(ViridianCore))]
        private static void Start()
        {
            Events.GMTrackingEvent += TrackingSetting;
        }

        [ModEnd(typeof(ViridianCore))]
        private static void End()
        {
            Events.GMTrackingEvent -= TrackingSetting;
        }

        private static bool Patched = false;

        private static void TrackingSetting(bool enabled)
        {
            if (enabled)
            {
                TrackingEnabled();
            }
            else
            {
                TrackingDisabled();
            }
        }

        private static void TrackingDisabled()
        {
            GMTracker.ClearAll();
        }

        private static void TrackingEnabled()
        {
            if (!Patched)
            {
                foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
                {
                    if ((gameObject.hideFlags & HideFlags.HideInHierarchy) != HideFlags.HideInHierarchy)
                    {
                        GMTracker.TrackObjectRecursive(gameObject);
                    }
                }
                UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) =>
                {
                    foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
                    {
                        if ((gameObject.hideFlags & HideFlags.HideInHierarchy) != HideFlags.HideInHierarchy)
                        {
                            GMTracker.TrackObjectRecursive(gameObject);
                        }
                    }
                };
                Patched = true;
                HarmonyInstance harmony = ModuleInitializer.GetViridianCoreHarmony() as HarmonyInstance;
                foreach (MethodInfo method in typeof(UnityEngine.Object).GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    if (method.Name.Contains("Instantiate"))
                    {
                        MethodInfo selection = method;
                        if (selection.ContainsGenericParameters)
                        {
                            selection = selection.MakeGenericMethod(typeof(UnityEngine.Object));
                        }
                        harmony.Patch(selection, null, new HarmonyMethod(typeof(GMTrackingPatches).GetMethod(nameof(GMTrackingPatches.InstantiatePostfix))));

                    }
                }
            }
        }
    }
}
