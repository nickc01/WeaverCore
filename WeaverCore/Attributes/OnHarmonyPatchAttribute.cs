using System;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Called for patching a method with a <see cref="WeaverCore.HarmonyPatcher"/>
    /// </summary>
    /// <example>
    /// <code>
    /// static bool Log_Prefix()
    ///	{
    ///		Debug.Log("Debug.Log has been patched!");
    ///		return true;
    ///	}
    ///
    /// [OnHarmonyPatch]
    /// static void Patch(HarmonyPatcher patcher)
    /// {
    ///     //This patches the Debug.Log method to print "Debug.Log has been patched" each time it's run
    ///     var orig = typeof(UnityEngine.Debug).GetMethod("Log");
    ///     var pre = typeof(DreamNailable).GetMethod("Log_Prefix", BindingFlags.Static | BindingFlags.NonPublic);
    ///     patcher.Patch(orig, pre, null);
    /// }
    /// </code>
    /// </example>
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnHarmonyPatchAttribute : PriorityAttribute
    {
        public OnHarmonyPatchAttribute(int priority = 0) : base(priority) { }
    }
}
