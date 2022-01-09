using System;
using WeaverCore.Features;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Called when a registry feature is loaded. Can be attached to a static method
    /// </summary>
    /// <example>
    /// <code>
    /// [OnFeatureLoad]
    /// static void FeatureLoaded(<seealso cref="LanguageTable"/> langTable)
    /// {
    ///     //This is called whenever a <seealso cref="LanguageTable"/> is loaded in a registry
    /// }
    /// </code>
    /// </example>
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnFeatureLoadAttribute : PriorityAttribute
    {
        public OnFeatureLoadAttribute(int priority = 0) : base(priority) { }
    }
}
