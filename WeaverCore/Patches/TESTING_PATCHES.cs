using System.Reflection;
using WeaverCore.Attributes;
using WeaverCore;
using UnityEngine;
using System;

public static class TESTING_PATCHES
{
    const bool ENABLED = false;

    /*static bool InstantiatePrefix(UnityEngine.Object data)
    {
        //WeaverLog.Log($"INSTANTIATE PREFIX RUN, data = {data}, type = {data?.GetType().Name}");
        if (data is ParticleSystem ps && ps.name.Contains("Acid Control"))
        {
            WeaverLog.Log("Instantiating object = " + ps);
            WeaverLog.Log(new System.Diagnostics.StackTrace());
        }
        return true;
    }*/

    [OnHarmonyPatch(-10)]
    static void OnInit(HarmonyPatcher patcher)
    {
        if (!ENABLED)
        {
            return;
        }




        //WeaverLog.Log("DOING TESTING PATCHES");

        /*{
            var orig = typeof(UnityEngine.Object).GetMethod("Internal_InstantiateSingle_Injected", BindingFlags.NonPublic | BindingFlags.Static);
            var prefix = typeof(TESTING_PATCHES).GetMethod("InstantiatePrefix", BindingFlags.NonPublic | BindingFlags.Static);

            patcher.Patch(orig, prefix, null);
        }

        {
            var orig = typeof(UnityEngine.Object).GetMethod("Internal_InstantiateSingleWithParent_Injected", BindingFlags.NonPublic | BindingFlags.Static);
            var prefix = typeof(TESTING_PATCHES).GetMethod("InstantiatePrefix", BindingFlags.NonPublic | BindingFlags.Static);

            patcher.Patch(orig, prefix, null);
        }*/
        /*{
            var orig = typeof(Transform).GetProperty(nameof(Transform.position), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).GetSetMethod();

            var prefix = typeof(TESTING_PATCHES).GetMethod(nameof(TransformPositionSetter_Prefix), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);

            patcher.Patch(orig, prefix, null);
        }

        {
            var orig = typeof(Transform).GetProperty(nameof(Transform.localPosition), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).GetSetMethod();

            var prefix = typeof(TESTING_PATCHES).GetMethod(nameof(TransformLocalPositionSetter_Prefix), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);

            patcher.Patch(orig, prefix, null);
        }*/

        /*{
            var orig = typeof(UnityEngine.Object).GetMethod(nameof(UnityEngine.Object.Destroy), BindingFlags.Static | BindingFlags.Public,null, new Type[] { typeof(UnityEngine.Object), typeof(float) }, null);

            var prefix = typeof(TESTING_PATCHES).GetMethod(nameof(Destroy_Prefix), BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public);

            patcher.Patch(orig, prefix, null);
        }*/
    }

    /*static bool Destroy_Prefix(UnityEngine.Object obj, float t)
    {
        WeaverLog.Log($"DESTROYING OBJ {obj} in {t} seconds");
        return true;
    }*/
}
