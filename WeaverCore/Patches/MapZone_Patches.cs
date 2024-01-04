using GlobalEnums;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using UnityEngine;
using WeaverCore;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.Utilities;

public static class MapZone_Patches
{
    [OnHarmonyPatch]
    static void Patch_Init(HarmonyPatcher patcher)
    {
        {
            var prefix = typeof(MapZone_Patches).GetMethod(nameof(MapZoneParsePrefix), BindingFlags.NonPublic | BindingFlags.Static);

            var orig = typeof(Enum).GetMethod(nameof(Enum.Parse),new Type[] { typeof(Type),typeof(string)});

            patcher.Patch(orig, prefix, null);
        }

        {
            var prefix = typeof(MapZone_Patches).GetMethod(nameof(MapZoneParsePrefixV2), BindingFlags.NonPublic | BindingFlags.Static);

            var orig = typeof(Enum).GetMethod(nameof(Enum.Parse), new Type[] { typeof(Type), typeof(string), typeof(bool) });

            patcher.Patch(orig, prefix, null);
        }

        {
            var prefix = typeof(MapZone_Patches).GetMethod(nameof(ToStringPrefix), BindingFlags.NonPublic | BindingFlags.Static);

            var orig = typeof(Enum).GetMethod(nameof(Enum.ToString),new Type[] { });

            patcher.Patch(orig, prefix, null);
        }
    }

    static bool ToStringPrefix(ref Enum __instance, ref string __result)
    {
        if (__instance.GetType() == typeof(MapZone))
        {
            var underlyingValue = Convert.ToInt32(Convert.ChangeType(__instance, Enum.GetUnderlyingType(__instance.GetType())));

            foreach (var zone in MapZoneUtilities.GetCustomMapZones())
            {
                if (zone.MapZoneID == underlyingValue)
                {
                    __result = zone.GetInternalName();
                    return false;
                }
            }
        }

        return true;
    }

    static bool MapZoneParsePrefix(Type enumType, string value, ref object __result)
    {
        if (enumType == typeof(MapZone))
        {
            foreach (var zone in MapZoneUtilities.GetCustomMapZones())
            {
                if (zone.GetInternalName() == value)
                {
                    __result = zone.MapZone;
                    return false;
                }
            }
        }
        return true;
    }

    static bool MapZoneParsePrefixV2(Type enumType, string value, bool ignoreCase, ref object __result)
    {
        if (enumType == typeof(MapZone))
        {
            foreach (var zone in MapZoneUtilities.GetCustomMapZones())
            {
                if (zone.GetInternalName() == value || (ignoreCase && zone.GetInternalName() == value.ToUpper()))
                {
                    __result = zone.MapZone;
                    return false;
                }
            }
        }
        return true;
    }
}

