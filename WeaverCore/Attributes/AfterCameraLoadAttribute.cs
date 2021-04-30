using System;

namespace WeaverCore.Attributes
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class AfterCameraLoadAttribute : PriorityAttribute
    {
        public AfterCameraLoadAttribute(int priority = 0) : base(priority)
		{

		}
    }
}
