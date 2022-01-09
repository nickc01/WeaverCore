using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Utilities;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Called after a <see cref="Modding.Mod"/> of a specific type has loaded. Can be attached to a static method
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
        /// The name of the assembly the type belongs to
        /// </summary>
        public readonly string AssemblyName;

        /// <summary>
        /// The full name of the mod type
        /// </summary>
        public readonly string TypeName;


        /// <summary>
        /// The type of mod that, when loaded, will call any methods with this attribute attached
        /// </summary>
        public Type ModType => TypeUtilities.NameToType(TypeName, AssemblyName);

        public AfterModLoadAttribute(Type type, int priority = 0) : base(priority)
        {
            AssemblyName = type.Assembly.FullName;
            TypeName = type.FullName;
        }

        /// <summary>
        /// The type of mod that, when loaded, will call any methods with this attribute attached
        /// </summary>
        /// <param name="typeName">The full name of the mod type</param>
        /// <param name="assemblyName">The assembly the mod type belongs to</param>
        public AfterModLoadAttribute(string typeName, string assemblyName)
        {
            AssemblyName = assemblyName;
            TypeName = typeName;
        }

        /// <summary>
        /// The type of mod that, when loaded, will call any methods with this attribute attached
        /// </summary>
        /// <param name="typeName">The full name of the mod type</param>
        public AfterModLoadAttribute(string typeName)
        {
            TypeName = typeName;
        }
    }
}
