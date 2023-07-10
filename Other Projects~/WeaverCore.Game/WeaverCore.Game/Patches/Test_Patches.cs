using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore.Game.Patches
{
    static class Test_Patches
    {
        /*[OnHarmonyPatch]
        static void Patch(HarmonyPatcher patcher)
        {
            var positionMethod = typeof(Transform).GetProperty(nameof(Transform.position)).GetSetMethod();
            var localPositionMethod = typeof(Transform).GetProperty(nameof(Transform.localPosition)).GetSetMethod();

            var printPositionMethod = typeof(Test_Patches).GetMethod(nameof(PrintPositionUpdate));
            var printLocalPositionMethod = typeof(Test_Patches).GetMethod(nameof(PrintLocalPositionUpdate));

            patcher.Patch(positionMethod, null, printPositionMethod);
            patcher.Patch(localPositionMethod, null, printLocalPositionMethod);
        }



        public static void PrintPositionUpdate(Transform __instance, Vector3 value)
        {
            if (__instance.name == "Knight")
            {
                WeaverLog.Log("Pos = " + value);
                WeaverLog.Log(new System.Diagnostics.StackTrace());
            }
        }

        public static void PrintLocalPositionUpdate(Transform __instance, Vector3 value)
        {
            if (__instance.name == "Knight")
            {
                WeaverLog.Log("Local Pos = " + value);
                WeaverLog.Log(new System.Diagnostics.StackTrace());
            }
        }*/
    }

}
