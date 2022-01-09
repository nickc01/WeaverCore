using System.Collections.Generic;

namespace WeaverCore.Interfaces
{
    /// <summary>
    /// Used to modify how the health value of an <see cref="WeaverCore.Components.EntityHealth"/> component changes
    /// </summary>
    public interface IHealthModifier
    {
        /// <summary>
        /// The priority of the health modifier. The lower the value, the sooner this modifier will get run before others
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Called anytime the health in a <see cref="WeaverCore.Components.EntityHealth"/> changes
        /// </summary>
        /// <param name="oldHealth">The previous health value</param>
        /// <param name="newHealth">The new health value</param>
        /// <returns>Returns a new value to set the health to</returns>
        int OnHealthChange(int oldHealth, int newHealth);

        public class Sorter : IComparer<IHealthModifier>
        {
            Comparer<int> numComparer = Comparer<int>.Default;

            public int Compare(IHealthModifier x, IHealthModifier y)
            {
                return numComparer.Compare(x.Priority, y.Priority);
            }
        }
    }
}
