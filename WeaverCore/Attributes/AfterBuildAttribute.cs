using System;

namespace WeaverCore.Attributes
{
	/// <summary>
	/// Called after the build process finished, or when the build process has failed. Can be attached to a static method
	/// </summary>
	[System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class AfterBuildAttribute : PriorityAttribute
    {
        public AfterBuildAttribute(int priority = 0) : base(priority) { }
    }
}
