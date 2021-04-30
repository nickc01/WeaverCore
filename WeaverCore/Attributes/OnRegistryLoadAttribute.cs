using System;

namespace WeaverCore.Attributes
{
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnRegistryLoadAttribute : PriorityAttribute
    {
        public OnRegistryLoadAttribute(int priority = 0) : base(priority)
        {

        }
    }
}
