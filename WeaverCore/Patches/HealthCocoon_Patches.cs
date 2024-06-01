

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

    static Action<HealthCocoon, bool> activatedSetter;
    static Func<HealthCocoon, bool> activatedGetter;
    static Action<HealthCocoon> SetBroken;

    [OnHarmonyPatch]
    static void Patch(HarmonyPatcher patcher)
    {
        activatedGetter = ReflectionUtilities.CreateFieldGetter<HealthCocoon, bool>("activated");
        activatedSetter = ReflectionUtilities.CreateFieldSetter<HealthCocoon, bool>("activated");
        SetBroken = ReflectionUtilities.MethodToDelegate<Action<HealthCocoon>>(typeof(HealthCocoon).GetMethod("SetBroken", BindingFlags.Instance | BindingFlags.NonPublic));
        
        //Awake()
        {
            var orig = typeof(HealthCocoon).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            var postfix = typeof(HealthCocoon_Patches).GetMethod(nameof(AwakePostfix), BindingFlags.NonPublic | BindingFlags.Static);

            patcher.Patch(orig, null, postfix);
        }

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

        //PlaySound()
        {
            var orig = typeof(HealthCocoon).GetMethod("PlaySound", BindingFlags.NonPublic | BindingFlags.Instance);
            var prefix = typeof(HealthCocoon_Patches).GetMethod(nameof(PlaySoundPrefix), BindingFlags.NonPublic | BindingFlags.Static);

            patcher.Patch(orig, prefix, null);
        }
    }

    static bool PlaySoundPrefix(HealthCocoon __instance, AudioClip clip)
    {
        if (__instance.TryGetComponent<VolumeToDistance>(out var vtd))
        {
            //WeaverLog.Log("PLAYING SOUND = " + clip + "at volume = " + vtd.CalculateVolumeAtPoint(Player.Player1.transform.position));
            WeaverAudio.PlayAtPoint(clip, __instance.transform.position, vtd.CalculateVolumeAtPoint(Player.Player1.transform.position));
            //WeaverAudio.AddVolumeDistanceControl(instance, vtd.DistanceMinMax);
            return false;
        }

        return true;
    }

    static void AwakePostfix(HealthCocoon __instance)
    {
        WeaverPersistentBoolItem component = __instance.GetComponent<WeaverPersistentBoolItem>();
        WeaverLog.Log("FOUND PERSISTENT COMPONENT = " + component);
        if (!component)
        {
            return;
        }
        component.OnGetSaveState += delegate (ref bool value)
        {
            value = activatedGetter(__instance);
        };
        component.OnSetSaveState += delegate (bool value)
        {
            //activated = value;
            activatedSetter(__instance, value);
            if (activatedGetter(__instance))
            {
                SetBroken(__instance);
            }
        };
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