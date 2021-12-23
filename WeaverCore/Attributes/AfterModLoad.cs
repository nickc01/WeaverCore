using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Called after a mod of a specific type has loaded. Can be attached to a static method
    /// </summary>
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
