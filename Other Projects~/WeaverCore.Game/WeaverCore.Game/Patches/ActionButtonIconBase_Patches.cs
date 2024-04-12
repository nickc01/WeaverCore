using System.Reflection;
using TMPro;
using WeaverCore.Attributes;

namespace WeaverCore.Game.Patches
{
    static class ActionButtonIconBase_Patches
    {
        [OnHarmonyPatch]
        static void OnHarmonyPatch(HarmonyPatcher patcher)
        {
            {
                var orig = typeof(ActionButtonIconBase).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
                var prefix = typeof(ActionButtonIconBase_Patches).GetMethod(nameof(Awake_Prefix), BindingFlags.Static | BindingFlags.NonPublic);
            }
        }

        static bool Awake_Prefix(ActionButtonIconBase __instance)
        {
            if (__instance.label == null)
            {
                __instance.label = __instance.GetComponentInChildren<TextMeshPro>();
            }

            if (__instance.textContainer == null)
            {
                __instance.textContainer = __instance.label.textContainer;
            }
            return true;
        }
    }

}
