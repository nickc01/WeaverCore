using System;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Called when the in-game camera has loaded. Can be attached to a static method
    /// </summary>
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class AfterCameraLoadAttribute : PriorityAttribute
    {
        public AfterCameraLoadAttribute(int priority = 0) : base(priority) { }
    }
}
