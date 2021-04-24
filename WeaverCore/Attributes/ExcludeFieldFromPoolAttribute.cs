using System;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Excludes a field from being reset by a pool
    /// </summary>
	[System.AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class ExcludeFieldFromPoolAttribute : Attribute
    {

    }
}
