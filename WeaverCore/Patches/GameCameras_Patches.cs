using System;
using System.Reflection;
using WeaverCore;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

public static class GameCameras_Patches
{
    [OnHarmonyPatch(-10)]
    static void OnInit(HarmonyPatcher patcher)
    {
        var orig = typeof(GameCameras).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);

        var prefix = typeof(GameCameras_Patches).GetMethod(nameof(StartPrefix), BindingFlags.NonPublic | BindingFlags.Static);

        patcher.Patch(orig, prefix, null);
    }

    static bool StartPrefix()
    {
        try
        {
            ReflectionUtilities.ExecuteMethodsWithAttribute<AfterGameCameraStartLoadAttribute>();
        }
        catch (Exception e)
        {
            WeaverLog.LogException(e);
        }
        return true;
    }
} 