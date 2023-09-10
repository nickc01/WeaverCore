

using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using WeaverCore;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.Utilities;

public static class HealthCocoon_Patches
{
    static Action<HealthCocoon, AudioClip> playSoundFunc;

    static Func<HealthCocoon, float> waitMinGetter;
    static Func<HealthCocoon, float> waitMaxGetter;

    static Func<HealthCocoon, Coroutine> animRoutineGetter;



    [OnHarmonyPatch]
    static void Patch(HarmonyPatcher patcher)
    {
        playSoundFunc = ReflectionUtilities.MethodToDelegate<Action<HealthCocoon, AudioClip>, HealthCocoon>("PlaySound");
        waitMinGetter = ReflectionUtilities.CreateFieldGetter<HealthCocoon, float>("waitMin");
        waitMaxGetter = ReflectionUtilities.CreateFieldGetter<HealthCocoon, float>("waitMax");
        animRoutineGetter = ReflectionUtilities.CreateFieldGetter<HealthCocoon, Coroutine>("animRoutine");

        //Animate()
        {
            var orig = typeof(HealthCocoon).GetMethod("Animate", BindingFlags.NonPublic | BindingFlags.Instance);
            var prefix = typeof(HealthCocoon_Patches).GetMethod(nameof(AnimatePrefix), BindingFlags.NonPublic | BindingFlags.Static);
            patcher.Patch(orig, prefix, null);
        }

        //SetBroken()
        {
            var orig = typeof(HealthCocoon).GetMethod("SetBroken", BindingFlags.NonPublic | BindingFlags.Instance);
            var prefix = typeof(HealthCocoon_Patches).GetMethod(nameof(SetBroken_Prefix), BindingFlags.NonPublic | BindingFlags.Static);
            patcher.Patch(orig, prefix, null);
        }
    }

    static bool AnimatePrefix(HealthCocoon __instance, ref IEnumerator __result)
    {
        if (__instance.GetComponent<WeaverAnimationPlayer>() != null)
        {
            __result = NewAnimateRoutine(__instance);
            return false;
        }
        return true;
    }

    static IEnumerator NewAnimateRoutine(HealthCocoon self)
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(waitMinGetter(self), waitMaxGetter(self)));
            playSoundFunc(self, self.moveSound);

            if (self.TryGetComponent<WeaverAnimationPlayer>(out var animator))
            {
                yield return animator.PlayAnimationTillDone(self.sweatAnimation);
                animator.PlayAnimation(self.idleAnimation);
            }
        }
    }

    static bool SetBroken_Prefix(HealthCocoon __instance)
    {
        __instance.StopCoroutine(animRoutineGetter(__instance));
        __instance.GetComponent<Renderer>().enabled = false;
        GameObject[] array = __instance.disableChildren;
        for (int i = 0; i < array.Length; i++)
        {
            array[i].SetActive(value: false);
        }
        Collider2D[] array2 = __instance.disableColliders;
        for (int i = 0; i < array2.Length; i++)
        {
            array2[i].enabled = false;
        }

        return false;
    }
}