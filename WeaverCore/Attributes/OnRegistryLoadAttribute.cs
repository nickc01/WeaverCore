using System;

namespace WeaverCore.Attributes
{
	/// <summary>
	/// Called when a registry is loaded. This can be attached to a static method
	/// </summary>
	/// <example>
	/// <code>
	/// [OnRegistryLoad]
	/// static void OnRegistryLoad(Registry registry)
	/// {
	/// 	//This gets called when a registry gets loaded
	/// }
	/// </code>
	/// </example>
	[System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnRegistryLoadAttribute : PriorityAttribute
    {
        public OnRegistryLoadAttribute(int priority = 0) : base(priority)
        {

        }

		
	}
}
