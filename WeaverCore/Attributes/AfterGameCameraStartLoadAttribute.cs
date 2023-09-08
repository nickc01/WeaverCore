using System;

namespace WeaverCore.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class AfterGameCameraStartLoadAttribute : PriorityAttribute
    {
        public AfterGameCameraStartLoadAttribute(int priority = 0) : base(priority) { }
    }
}
