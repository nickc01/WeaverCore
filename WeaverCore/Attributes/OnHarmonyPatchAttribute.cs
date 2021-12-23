using System;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Called for patching a method with a <see cref="WeaverCore.Utilities.HarmonyPatcher"/>
    /// </summary>
	[System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnHarmonyPatchAttribute : PriorityAttribute
    {
        public OnHarmonyPatchAttribute(int priority = 0) : base(priority) { }
    }
}
