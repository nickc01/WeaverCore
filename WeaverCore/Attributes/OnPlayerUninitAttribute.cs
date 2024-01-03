using System;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Called when the player script has uninitialized
    /// </summary>
    /// <example>
    /// <code>
    /// [OnPlayerUninit]
    /// static void OnPlayerUninit(<seealso cref="Player"/> unloadedPlayer)
    /// {
    ///     //This is called whenever a <seealso cref="Player"/> is unloaded
    /// }
    /// </code>
    /// </example>
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnPlayerUninitAttribute : PriorityAttribute
    {
        public OnPlayerUninitAttribute(int priority = 0) : base(priority)
        {

        }
    }
}
