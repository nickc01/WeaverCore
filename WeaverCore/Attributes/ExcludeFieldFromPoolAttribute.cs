using System;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Excludes a field from being modified by an <see cref="ObjectPool"/>
    /// </summary>
	[System.AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class ExcludeFieldFromPoolAttribute : Attribute { }
}
