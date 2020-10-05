using System;

namespace WeaverCore.Attributes
{
	[System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnInitAttribute : PriorityAttribute
    {
        public OnInitAttribute(int priority = 0) : base(priority)
        {

        }
    }
}
