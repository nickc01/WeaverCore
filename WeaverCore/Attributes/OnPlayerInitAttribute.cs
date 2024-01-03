using System;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Called when the player script has initialized
    /// </summary>
    /// <example>
    /// <code>
    /// [OnPlayerInit]
    /// static void OnPlayerInit(<seealso cref="Player"/> loadedPlayer)
    /// {
    ///     //This is called whenever a <seealso cref="Player"/> is loaded
    /// }
    /// </code>
    /// </example>
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnPlayerInitAttribute : PriorityAttribute
    {
        public OnPlayerInitAttribute(int priority = 0) : base(priority)
        {

        }
    }
}
