using System;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// In the Unity Editor, this is called when the Unity Editor is loaded
    /// 
    /// In Hollow Knight, this is called when the game starts
    /// 
    /// This can be attached to a static method
    /// </summary>
	[System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnInitAttribute : PriorityAttribute
    {
        public OnInitAttribute(int priority = 0) : base(priority)
        {

        }
    }
}
