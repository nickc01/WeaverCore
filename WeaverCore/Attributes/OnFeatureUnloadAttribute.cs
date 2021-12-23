using System;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Called when a registry feature is unloaded. Can be attached to a static method
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnFeatureUnloadAttribute : PriorityAttribute
    {
        public OnFeatureUnloadAttribute(int priority = 0) : base(priority) { }
    }
}
