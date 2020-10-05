using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class AfterModLoadAttribute : PriorityAttribute
    {
        public readonly Type ModType;


        public AfterModLoadAttribute(Type type, int priority = 0) : base(priority)
        {
            ModType = type;
        }
    }
}
