using System;

namespace WeaverCore.Attributes
{
	/// <summary>
	/// Called when a registry is unloaded
	/// </summary>
	[System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnRegistryUnloadAttribute : PriorityAttribute
    {
        public OnRegistryUnloadAttribute(int priority = 0) : base(priority)
        {

        }
    }
}
