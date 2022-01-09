using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Adding this attribute to a class allows it to be added to a registry
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class ShowFeatureAttribute : Attribute
    {

    }
}
