using System;

namespace WeaverCore.Attributes
{
	[System.AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class ExcludeFieldFromPoolAttribute : Attribute
    {

    }
}
