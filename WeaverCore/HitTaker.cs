using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Implementations;

namespace WeaverCore
{
    /// <summary>
    /// Used for dealing damage to enemies
    /// </summary>
    public static class HitTaker
    {
        static HitTaker_I impl = ImplFinder.GetImplementation<HitTaker_I>();

        /// <summary>
        /// Attempts to hit an enemy. Returns true if damage has been dealt
        /// </summary>
        /// <param name="target">The target enemy to damage</param>
        /// <param name="attacker">The object doing the attack</param>
        /// <param name="damage">The amount of damage to deal</param>
        /// <param name="type">The type of attack being dealt</param>
        /// <param name="hitDirection">The direction of the attack</param>
        /// <returns>Returns true if damage has been dealt</returns>
        public static bool Hit(Transform target, GameObject attacker, int damage, AttackType type, CardinalDirection hitDirection)
        {
            return impl.Hit(target, attacker, damage, type, hitDirection);
        }
    }
}
