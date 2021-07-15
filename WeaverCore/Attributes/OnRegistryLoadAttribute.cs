using System;

namespace WeaverCore.Attributes
{
	/// <summary>
	/// Called when a registry is loaded
	/// </summary>
	[System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnRegistryLoadAttribute : PriorityAttribute
    {
        public OnRegistryLoadAttribute(int priority = 0) : base(priority)
        {

        }
    }
}
