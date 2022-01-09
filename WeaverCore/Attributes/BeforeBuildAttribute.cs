using System;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Called before the mod build process begins. Can be attached to a static method
    /// </summary>
    /// <example>
    /// <code>
    /// [BeforeBuild()]
    /// public static void BeforeModBuild()
    /// {
    ///     //This will be called before the mod build process begins
    /// }
    /// </code>
    /// </example>
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class BeforeBuildAttribute : PriorityAttribute
    {
        public BeforeBuildAttribute(int priority = 0) : base(priority) { }
    }
}
