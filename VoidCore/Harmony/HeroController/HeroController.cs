using Harmony;
using VoidCore.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace VoidCore.Harmony
{
    [HarmonyPatch(typeof(HeroController))]
    [HarmonyPatch("Start")]
    internal class HeroControllerStart
    {
        static void Postfix(HeroController __instance)
        {
            ModLog.Log("HERO START");
            foreach (var hook in PlayerHook.AvailableHooks)
            {
                __instance.gameObject.AddComponent(hook);
            }
        }
    }
}
