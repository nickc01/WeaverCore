using System;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Called when the in-game camera has started. Can be attached to a static method. Note: This method is called one frame after the AfterCameraLoad attribute is triggered
    /// </summary>
    /// <example>
    /// <code>
    /// [AfterGameCameraStartLoad()]
    /// public static void CameraIsLoaded()
    /// {
    ///     //This will be called when the in-game camera has started
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class AfterGameCameraStartLoadAttribute : PriorityAttribute
    {
        public AfterGameCameraStartLoadAttribute(int priority = 0) : base(priority) { }
    }
}
