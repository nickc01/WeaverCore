using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViridianCore
{

    /// <summary>
    /// When added onto a static method, that method will be called when a mod is loaded
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    internal sealed class ModStartAttribute : Attribute
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
        public ModStartAttribute(Type type = null)
        {
            ModType = type ?? typeof(object);
        }
    }
}
