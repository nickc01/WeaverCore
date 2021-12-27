using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Called after a <see cref="WeaverMod"/> of a specific type has loaded. Can be attached to a static method
    /// </summary>
    /// <example>
    /// <code>
    /// [AfterModLoad(typeof(CorruptedKin))]
    /// public static void ModIsLoaded()
    /// {
    ///     //This will be called after Corrupted Kin is loaded
    /// }
    /// </code>
    /// </example>
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class AfterModLoadAttribute : PriorityAttribute
    {
        /// <summary>
        /// The type of mod that, when loaded, will call any methods with this attribute attached
        /// </summary>
        public readonly Type ModType;

        public AfterModLoadAttribute(Type type, int priority = 0) : base(priority)
        {
            ModType = type;
        }
    }
}
