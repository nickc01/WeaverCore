using System;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Called after the mod build process finished, or when the build process has failed. Can be attached to a static method
    /// </summary>
    /// <example>
    /// <code>
    /// [AfterBuild()]
    /// public static void BuildIsFinished()
    /// {
    ///     //This will be called after the mod build process has finished
    /// }
    /// </code>
    /// </example>
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class AfterBuildAttribute : PriorityAttribute
    {
        public AfterBuildAttribute(int priority = 0) : base(priority) { }
    }
}
