using System;
using System.Linq;
using System.Reflection;
using WeaverCore;
using WeaverCore.Attributes;

public static class PaleCourt_Patches
{
    [OnHarmonyPatch]
    static void OnHarmonyPatch(HarmonyPatcher patcher)
    {
        var paleCourtAsm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "PaleCourt");

        if (paleCourtAsm != null)
        {
            WeaverLog.Log("FOUND PALE COURT");

            var fiveKnightsType = paleCourtAsm.GetType("FiveKnights.FiveKnights");

            var orig = fiveKnightsType.GetMethod("LoadPaleCourtMenuMusic", BindingFlags.NonPublic | BindingFlags.Instance);

            var prefix = typeof(PaleCourt_Patches).GetMethod(nameof(Prefix), BindingFlags.NonPublic | BindingFlags.Static);

            patcher.Patch(orig, prefix, null);
        }
    }

    static bool Prefix(MusicCue musicCue)
    {
        return !(musicCue is WeaverMusicCue);
    }
}
