using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// An attribute that allows you to specify a priority, changing the order this attribute gets processed
    /// </summary>
    public abstract class PriorityAttribute : Attribute
    {
        /// <summary>
        /// Determines the order the init function is run. THe lower the number, the sooner the function gets called before others
        /// </summary>
        public readonly int Priority = 0;


        public PriorityAttribute(int priority = 0)
        {
            Priority = priority;
        }

        public class Sorter<T> : IComparer<ValueTuple<MethodInfo, T>> where T : Attribute
        {
            Comparer<int> numericalComparer = Comparer<int>.Default;

            int IComparer<ValueTuple<MethodInfo, T>>.Compare(ValueTuple<MethodInfo, T> x, ValueTuple<MethodInfo, T> y)
            {
                if (x.Item2 is PriorityAttribute)
                {
                    return numericalComparer.Compare((x.Item2 as PriorityAttribute).Priority, (y.Item2 as PriorityAttribute).Priority);
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
