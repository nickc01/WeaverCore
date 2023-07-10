using GlobalEnums;
using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using WeaverCore;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.Utilities;

public static class HeroBox_Patches
{
    static Action<HeroBox, int> damageDealtSet;
    static Action<HeroBox, int> hazardTypeSet;
    static Action<HeroBox, GameObject> damagingObjectSet;
    static Action<HeroBox, CollisionSide> collisionSideSet;
    static Action<HeroBox, bool> isHitBufferedSet;

    static Func<int, bool> IsHitTypeBuffered;
    static Action<HeroBox> ApplyBufferedHit;

    [OnHarmonyPatch]
    static void Patch(HarmonyPatcher patcher)
    {
        var checkForDamage_Orig = typeof(HeroBox).GetMethod("CheckForDamage", BindingFlags.NonPublic | BindingFlags.Instance);

        var checkForDamage_Prefix = typeof(HeroBox_Patches).GetMethod(nameof(CheckForDamagePrefix), BindingFlags.NonPublic | BindingFlags.Static);

        patcher.Patch(checkForDamage_Orig, checkForDamage_Prefix, null);
        damageDealtSet = ReflectionUtilities.CreateFieldSetter<HeroBox, int>("damageDealt");
        hazardTypeSet = ReflectionUtilities.CreateFieldSetter<HeroBox, int>("hazardType");
        damagingObjectSet = ReflectionUtilities.CreateFieldSetter<HeroBox, GameObject>("damagingObject");
        collisionSideSet = ReflectionUtilities.CreateFieldSetter<HeroBox, CollisionSide>("collisionSide");
        isHitBufferedSet = ReflectionUtilities.CreateFieldSetter<HeroBox, bool>("isHitBuffered");
        IsHitTypeBuffered = ReflectionUtilities.MethodToDelegate<Func<int, bool>>(typeof(HeroBox).GetMethod(nameof(IsHitTypeBuffered), BindingFlags.NonPublic | BindingFlags.Static));
        ApplyBufferedHit = ReflectionUtilities.MethodToDelegate<Action<HeroBox>>(typeof(HeroBox).GetMethod(nameof(ApplyBufferedHit), BindingFlags.NonPublic | BindingFlags.Instance));
    }

    static bool CheckForDamagePrefix(HeroBox __instance, Collider2D otherCollider)
    {
        if (otherCollider.TryGetComponent<ForcePlayerDamager>(out var forceDamager))
        {
            //WeaverLog.Log("HAS FORCED BUFFER HIT");
            damageDealtSet(__instance, forceDamager.damageDealt);
            hazardTypeSet(__instance, (int)forceDamager.hazardType);
            damagingObjectSet(__instance, otherCollider.gameObject);
            collisionSideSet(__instance, (otherCollider.gameObject.transform.position.x > __instance.transform.position.x) ? CollisionSide.right : CollisionSide.left);

            if (IsHitTypeBuffered((int)forceDamager.hazardType))
            {
                //WeaverLog.Log("FORCING BUFFERED HIT");
                ApplyBufferedHit(__instance);
            }
            else
            {
                //WeaverLog.Log("PREPARING FORCED BUFFER HIT");
                isHitBufferedSet(__instance, true);
            }
            return false;

            /*damageDealt = forceDamager.damageDealt;
            hazardType = forceDamager.hazardType;
            damagingObject = otherCollider.gameObject;
            collisionSide = ((otherCollider.gameObject.transform.position.x > __instance.transform.position.x) ? CollisionSide.right : CollisionSide.left);
            if (!HeroBox.IsHitTypeBuffered(hazardType))
            {
                ApplyBufferedHit();
                return false;
            }
            isHitBuffered = true;
            return false;*/
        }

        return true;
    }
}
