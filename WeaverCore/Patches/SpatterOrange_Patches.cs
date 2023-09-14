using System.Reflection;
using System;
using WeaverCore;
using WeaverCore.Attributes;
using WeaverCore.Utilities;
using UnityEngine;

public static class SpatterOrange_Patches
{
    static Func<SpatterOrange, float> stateGetter;
    static Func<SpatterOrange, float> animTimerGetter;
    static Func<SpatterOrange, int> animFrameGetter;
    static Action<SpatterOrange, int> animFrameSetter;

    [OnHarmonyPatch]
    static void Onpatch(HarmonyPatcher patcher)
    {
        stateGetter = ReflectionUtilities.CreateFieldGetter<SpatterOrange, float>("state");
        animTimerGetter = ReflectionUtilities.CreateFieldGetter<SpatterOrange, float>("animTimer");
        animFrameGetter = ReflectionUtilities.CreateFieldGetter<SpatterOrange, int>("animFrame");
        animFrameSetter = ReflectionUtilities.CreateFieldSetter<SpatterOrange, int>("animFrame");

        var orig = typeof(SpatterOrange).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);

        var prefix = typeof(GameCameras_Patches).GetMethod(nameof(UpdatePrefix), BindingFlags.NonPublic | BindingFlags.Static);

        var postfix = typeof(GameCameras_Patches).GetMethod(nameof(UpdatePostfix), BindingFlags.NonPublic | BindingFlags.Static);

        patcher.Patch(orig, prefix, postfix);
    }

    static bool UpdatePrefix(SpatterOrange __instance, ref bool __state)
    {
        __state = false;
        if (stateGetter(__instance) == -1f)
        {
            var animTimer = animTimerGetter(__instance) + Time.deltaTime;
            if (animTimer >= 1f / __instance.fps)
            {
                var animFrame = animFrameGetter(__instance) + 1;

                if (animFrame > 6)
                {
                    __state = true;
                    animFrameSetter(__instance, 0);
                    __instance.spriteRenderer.enabled = false;
                }
            }
        }
        return true;
    }

    static void UpdatePostfix(SpatterOrange __instance, ref bool __state)
    {
        if (__state)
        {
            __instance.spriteRenderer.enabled = true;
            __instance.spriteRenderer.sprite = __instance.sprites[6];
            animFrameSetter(__instance, 7);
            Pooling.Destroy(__instance);
        }
    }
}
