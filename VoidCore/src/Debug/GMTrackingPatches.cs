using Harmony;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using VoidCore;
using UnityEngine.SceneManagement;
using VoidCore.Hooks;

internal static class GMTrackingPatches
{
    [HarmonyPatch]
    internal static class ConstructorPatch1
    {
        static MethodBase TargetMethod()
        {
            return typeof(GameObject).GetConstructor(new Type[] { });
        }

        static void Postfix(GameObject __instance)
        {
            GMTracker.AddObject(__instance);
        }
    }

    [HarmonyPatch]
    internal static class ConstructorPatch2
    {
        static MethodBase TargetMethod()
        {
            return typeof(GameObject).GetConstructor(new Type[] { typeof(string),typeof(Type[]) });
        }

        static void Postfix(GameObject __instance)
        {
            GMTracker.AddObject(__instance);
        }
    }

    [HarmonyPatch]
    internal static class ConstructorPatch3
    {
        static MethodBase TargetMethod()
        {
            return typeof(GameObject).GetConstructor(new Type[] { typeof(string) });
        }

        static void Postfix(GameObject __instance)
        {
            GMTracker.AddObject(__instance);
        }
    }

    public static void InstantiatePostfix(UnityEngine.Object __result)
    {
        if (Settings.GMTracking)
        {
            if (__result is GameObject gm)
            {
                TrackingStarter.AddObject(gm);
            }
            else if (__result is Component c)
            {
                TrackingStarter.AddObject(c.gameObject);
            }
        }
    }

}

