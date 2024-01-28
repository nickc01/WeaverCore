﻿using System.Reflection;
using WeaverCore.Attributes;
using WeaverCore;
using UnityEngine;

public static class TESTING_PATCHES
{
    const bool ENABLED = false;


    [OnHarmonyPatch(-10)]
    static void OnInit(HarmonyPatcher patcher)
    {
        if (!ENABLED)
        {
            return;
        }
        {
            var orig = typeof(Transform).GetProperty(nameof(Transform.position), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).GetSetMethod();

            var prefix = typeof(TESTING_PATCHES).GetMethod(nameof(TransformPositionSetter_Prefix), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);

            patcher.Patch(orig, prefix, null);
        }

        {
            var orig = typeof(Transform).GetProperty(nameof(Transform.localPosition), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).GetSetMethod();

            var prefix = typeof(TESTING_PATCHES).GetMethod(nameof(TransformLocalPositionSetter_Prefix), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);

            patcher.Patch(orig, prefix, null);
        }
    }

    static bool TransformPositionSetter_Prefix(Transform __instance, Vector3 value)
    {
        if (__instance.name == "Amoh" || __instance.name == "Tele Sprite")
        {
            WeaverLog.Log($"Setting {__instance.name} Transform.position = " + value + "\n" + new System.Diagnostics.StackTrace());
        }
        return true;
    }

    static bool TransformLocalPositionSetter_Prefix(Transform __instance, Vector3 value)
    {
        if (__instance.name == "Amoh" || __instance.name == "Tele Sprite")
        {
            WeaverLog.Log($"Setting {__instance.name} Transform.localPosition = " + value + "\n" + new System.Diagnostics.StackTrace());
        }
        return true;
    }
}
