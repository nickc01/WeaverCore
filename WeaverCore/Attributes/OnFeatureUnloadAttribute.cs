using System;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Called when a registry feature is unloaded. Can be attached to a static method
    /// </summary>
    /// <example>
    /// <code>
    /// [OnFeatureUnload]
    /// static void FeatureUnloaded(<seealso cref="WeaverCore.Features.LanguageTable"/> langTable)
    /// {
    ///     //This is called whenever a <seealso cref="WeaverCore.Features.LanguageTable"/> is unloaded from a registry
    /// }
    /// </code>
    /// </example>
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnFeatureUnloadAttribute : PriorityAttribute
    {
        public OnFeatureUnloadAttribute(int priority = 0) : base(priority) { }
    }
}
