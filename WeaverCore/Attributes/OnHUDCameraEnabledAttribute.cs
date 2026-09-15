using System;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Called whenever the HUD camera is enabled.
    /// Can be attached to a static method with no parameters or a HUDCamera parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnHUDCameraEnabledAttribute : PriorityAttribute
    {
        public OnHUDCameraEnabledAttribute(int priority = 0) : base(priority)
        {
        }
    }
}
