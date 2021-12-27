using System;

namespace WeaverCore.Attributes
{
	/// <summary>
	/// Called when a registry is unloaded. This can be attached to a static method
	/// </summary>
	/// <example>
	/// <code>
	/// [OnRegistryUnload]
	/// static void OnRegistryUnload(Registry registry)
	/// {
	/// 	//This gets called when a registry gets unloaded
	/// }
	/// </code>
	/// </example>
	[System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnRegistryUnloadAttribute : PriorityAttribute
    {
        public OnRegistryUnloadAttribute(int priority = 0) : base(priority)
        {

        }
    }
}
