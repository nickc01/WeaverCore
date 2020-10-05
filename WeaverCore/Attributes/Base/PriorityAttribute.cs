using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WeaverCore.Attributes
{
    //[System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
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


        public class Sorter : IComparer<PriorityAttribute>
        {
            Comparer<int> numericalComparer = Comparer<int>.Default;

            public int Compare(PriorityAttribute x, PriorityAttribute y)
            {
                return numericalComparer.Compare(x.Priority, y.Priority);
            }

            static Sorter _default;
            public static Sorter Default
            {
                get
                {
                    if (_default == null)
                    {
                        _default = new Sorter();
                    }
                    return _default;
                }
            }
        }

        public class PrioritySorter<T> : IComparer<ValueTuple<MethodInfo, T>> where T : PriorityAttribute
        {
            Comparer<int> numericalComparer = Comparer<int>.Default;

            int IComparer<ValueTuple<MethodInfo, T>>.Compare(ValueTuple<MethodInfo, T> x, ValueTuple<MethodInfo, T> y)
            {
                return numericalComparer.Compare(x.Item2.Priority, y.Item2.Priority);
                /*if (x.Item2 is PriorityAttribute)
                {
                    return numericalComparer.Compare((x.Item2 as PriorityAttribute).Priority, (y.Item2 as PriorityAttribute).Priority);
                }
                else
                {
                    //return numericalComparer.Compare(x.Item2.Priority, y.Item2.Priority);
                    return 0;
                }*/
            }
        }

        public class PairSorter<T> : IComparer<ValueTuple<MethodInfo, T>> where T : Attribute
        {
            Comparer<int> numericalComparer = Comparer<int>.Default;

            int IComparer<ValueTuple<MethodInfo, T>>.Compare(ValueTuple<MethodInfo, T> x, ValueTuple<MethodInfo, T> y)
            {
                //return numericalComparer.Compare((x.Item2 as PriorityAttribute).Priority, (y.Item2 as PriorityAttribute).Priority);
                if (x.Item2 is PriorityAttribute)
                {
                    return numericalComparer.Compare((x.Item2 as PriorityAttribute).Priority, (y.Item2 as PriorityAttribute).Priority);
                }
                else
                {
                    //return numericalComparer.Compare(x.Item2.Priority, y.Item2.Priority);
                    return 0;
                }
            }
        }
    }
}
