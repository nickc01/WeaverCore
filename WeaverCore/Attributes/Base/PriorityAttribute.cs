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

        /// <summary>
        /// Sorts pairs of methods/attributes by their priority
        /// </summary>
        /// <typeparam name="T">The type of attributes to be sorted</typeparam>
        public class MethodSorter<T> : IComparer<ValueTuple<MethodInfo, T>> where T : Attribute
        {
            Comparer<int> numericalComparer = Comparer<int>.Default;

            int IComparer<ValueTuple<MethodInfo, T>>.Compare(ValueTuple<MethodInfo, T> x, ValueTuple<MethodInfo, T> y)
            {
                if (x.Item2 is PriorityAttribute xPriority && y.Item2 is PriorityAttribute yPriority)
                {
                    return numericalComparer.Compare(xPriority.Priority, yPriority.Priority);
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Sorts attributes by their priority
        /// </summary>
        /// <typeparam name="T">The type of attribute to be sorted</typeparam>
        public class Sorter<T> : IComparer<T> where T : Attribute
        {
            Comparer<int> numericalComparer = Comparer<int>.Default;

            int IComparer<T>.Compare(T x, T y)
            {
                if (x is PriorityAttribute xPriority && y is PriorityAttribute yPriority)
                {
                    return numericalComparer.Compare(xPriority.Priority, yPriority.Priority);
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
