using System;

namespace WeaverCore.Attributes
{
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnWeaverCoreAssemblyFoundAttribute : PriorityAttribute
    {
        public OnWeaverCoreAssemblyFoundAttribute(int priority = 0) : base(priority) { }
    }
}
