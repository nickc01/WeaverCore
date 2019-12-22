using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;

namespace CrystalCore.Harmony
{
    [HarmonyPatch(typeof(HealthManager))]
    [HarmonyPatch("Hit")]
    [HarmonyPatch(new Type[] { typeof(HitInstance)})]
    internal class HealthManagerHit
    {
        //HealthManager __instance,HitInstance hitInstance
        static bool Prefix()
        {
            ModLog.Log("ENEMY IS HIT");
            return true;
        }
    }
}
