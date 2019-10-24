using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoidCore
{

    /// <summary>
    /// When added onto a static method, that method will be called when any mod in the current assembly is loaded
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class OnModStartAttribute : Attribute
    {
        /// <summary>
        /// Specifies the mod that, when it's loaded, will call the method that is attached with this attribute
        /// </summary>
        public Type ModType { get; private set; }

        /// <summary>
        /// Constructs the attribute
        /// </summary>
        /// <param name="type">Specifies the mod that, when it's loaded, will call the method that is attached with this attribute.
        /// If the type is null, then the function will be called when the first mod loads</param>
        public OnModStartAttribute(Type type = null)
        {
            ModType = type ?? typeof(object);
        }
    }
}
