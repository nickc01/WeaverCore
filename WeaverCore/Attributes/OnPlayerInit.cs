using System;

namespace WeaverCore.Attributes
{
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnPlayerInit : PriorityAttribute
    {
        public OnPlayerInit(int priority = 0) : base(priority)
        {

        }
    }
}
