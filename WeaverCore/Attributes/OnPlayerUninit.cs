using System;

namespace WeaverCore.Attributes
{
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnPlayerUninit : PriorityAttribute
    {
        public OnPlayerUninit(int priority = 0) : base(priority)
        {

        }
    }
}
