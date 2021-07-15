using System;

namespace WeaverCore.Attributes
{
	/// <summary>
	/// Called when a registry feature is loaded
	/// </summary>
	[System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnFeatureLoadAttribute : PriorityAttribute
    {
        public OnFeatureLoadAttribute(int priority = 0) : base(priority)
        {

        }
    }
}
