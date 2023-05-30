

using System;
using System.Reflection;
using UnityEngine;
using WeaverCore;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.Utilities;

public static class ObjectBounce_Patches
{
    static Func<ObjectBounce, Rigidbody2D> rb;
    static Func<ObjectBounce, bool> bouncing;
    static Func<ObjectBounce, float> speed;
    static Func<ObjectBounce, float> getAnimTimer;
    static Action<ObjectBounce, float> setAnimTimer;

    class Patch_State
    {
        public bool persistTimer;
        public float timerValue;
    }


    static bool Collision_Prefix(ObjectBounce __instance, ref Patch_State __state)
    {
        var instanceRB = rb(__instance);
        if (!instanceRB || instanceRB.isKinematic || !bouncing(__instance) || !(speed(__instance) > __instance.speedThreshold))
        {
            return true;
        }

        //__state.persistTimer = false;


        if (__instance.playAnimationOnBounce)
        {
            var timer = getAnimTimer(__instance);
            if (timer <= 0f)
            {
                if (__instance.TryGetComponent<WeaverAnimationPlayer>(out var weaverAnimator))
                {
                    weaverAnimator.PlayAnimation(__instance.animationName);
                }


                var animator = __instance.GetComponent("tk2dSpriteAnimator");
                if (animator == null)
                {
                    //tk2dAnimator will be skipped
                    __state = new Patch_State();
                    __state.persistTimer = true;
                    __state.timerValue = __instance.animPause;
                    setAnimTimer(__instance, float.PositiveInfinity);
                }
            }
        }

        return true;
    }

    static void Collision_Postfix(ObjectBounce __instance, ref Patch_State __state)
    {
        if (__state != null && __state.persistTimer)
        {
            setAnimTimer(__instance, __state.timerValue);
        }
    }

    [OnHarmonyPatch]
    static void Patch_Init(HarmonyPatcher patcher)
    {
        var prefix = typeof(ObjectBounce_Patches).GetMethod(nameof(Collision_Prefix), BindingFlags.Static | BindingFlags.NonPublic);

        var postfix = typeof(ObjectBounce_Patches).GetMethod(nameof(Collision_Postfix), BindingFlags.Static | BindingFlags.NonPublic);

        rb = ReflectionUtilities.CreateFieldGetter<ObjectBounce, Rigidbody2D>(nameof(rb));
        bouncing = ReflectionUtilities.CreateFieldGetter<ObjectBounce, bool>(nameof(bouncing));
        speed = ReflectionUtilities.CreateFieldGetter<ObjectBounce, float>(nameof(speed));
        getAnimTimer = ReflectionUtilities.CreateFieldGetter<ObjectBounce, float>("animTimer");
        setAnimTimer = ReflectionUtilities.CreateFieldSetter<ObjectBounce, float>("animTimer");

        var method = typeof(ObjectBounce).GetMethod("OnCollisionEnter2D", BindingFlags.NonPublic | BindingFlags.Instance);

        patcher.Patch(method, prefix, postfix);
    }
}