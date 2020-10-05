using System;

namespace WeaverCore.Attributes
{
	[System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnHarmonyPatchAttribute : PriorityAttribute
    {
        public OnHarmonyPatchAttribute(int priority = 0) : base(priority)
        {

        }
    }
}
