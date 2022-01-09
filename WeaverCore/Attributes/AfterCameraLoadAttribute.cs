using System;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Called when the in-game camera has loaded. Can be attached to a static method
    /// </summary>
    /// <example>
    /// <code>
    /// [AfterCameraLoad()]
    /// public static void CameraIsLoaded()
    /// {
    ///     //This will be called when the in-game camera has loaded
    /// }
    /// </code>
    /// </example>
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class AfterCameraLoadAttribute : PriorityAttribute
    {
        public AfterCameraLoadAttribute(int priority = 0) : base(priority) { }
    }
}
