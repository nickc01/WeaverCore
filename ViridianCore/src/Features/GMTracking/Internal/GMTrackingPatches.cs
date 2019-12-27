using Harmony;
using System;
using System.Reflection;
using UnityEngine;
using ViridianCore;

internal static class GMTrackingPatches
{
    [HarmonyPatch]
    internal static class ConstructorPatch1
    {
        private static MethodBase TargetMethod()
        {
            return typeof(GameObject).GetConstructor(new Type[] { });
        }

        private static void Postfix(GameObject __instance)
        {
            GMTracker.TrackObject(__instance);
        }
    }

    [HarmonyPatch]
    internal static class ConstructorPatch2
    {
        private static MethodBase TargetMethod()
        {
            return typeof(GameObject).GetConstructor(new Type[] { typeof(string), typeof(Type[]) });
        }

        private static void Postfix(GameObject __instance)
        {
            GMTracker.TrackObject(__instance);
        }
    }

    [HarmonyPatch]
    internal static class ConstructorPatch3
    {
        private static MethodBase TargetMethod()
        {
            return typeof(GameObject).GetConstructor(new Type[] { typeof(string) });
        }

        private static void Postfix(GameObject __instance)
        {
            GMTracker.TrackObject(__instance);
        }
    }

    public static void InstantiatePostfix(UnityEngine.Object __result)
    {
        if (Settings.GMTracking)
        {
            if (__result is GameObject gm)
            {
                GMTracker.TrackObjectRecursive(gm);
            }
            else if (__result is Component c)
            {
                GMTracker.TrackObjectRecursive(c.gameObject);
            }
        }
    }

}

