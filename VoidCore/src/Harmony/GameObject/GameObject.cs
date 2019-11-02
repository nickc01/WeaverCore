using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VoidCore.Harmony
{
    /*[HarmonyPatch(typeof(GameObject))]
    [HarmonyPatch("Internal_CreateGameObject")]
    [HarmonyPatch(new Type[] {typeof(GameObject),typeof(string) })]
    internal class GameObjectCreate
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            return instr;
        }
    }*/

    /*[HarmonyPatch(typeof(GameObject))]
    internal class GameObjectCtor1
    {
        static void Postfix(GameObject __instance)
        {
            ModLog.Log("GameObject Created 1 = " + __instance.name);
        }
    }

    [HarmonyPatch(typeof(GameObject))]
    [HarmonyPatch(new Type[] { typeof(string)})]
    internal class GameObjectCtor2
    {
        static void Postfix(GameObject __instance)
        {
            ModLog.Log("GameObject Created 2 = " + __instance.name);
        }
    }

    [HarmonyPatch(typeof(GameObject))]
    [HarmonyPatch(new Type[] { typeof(string),typeof(Type[]) })]
    internal class GameObjectCtor3
    {
        static void Postfix(GameObject __instance)
        {
            ModLog.Log("GameObject Created 3 = " + __instance.name);
        }
    }*/
}
