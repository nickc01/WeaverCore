using GlobalEnums;
using UnityEngine;

namespace WeaverCore.Interfaces
{
    /// <summary>
    /// Used to adjust the damage the player receives
    /// </summary>
    public interface IPlayerDamageModifier
	{
        /// <summary>
        /// Called when the player gets hit by something. Use this to modify the different damage parameters
        /// </summary>
        /// <param name="__instance">The hero taking damage</param>
        /// <param name="damageSide">The direction the damage is coming from</param>
        /// <param name="damageAmount">The amount of damage to deal</param>
        /// <param name="hazardType">The type of damage being dealt</param>
        void OnPlayerHit(HeroController __instance, ref CollisionSide damageSide, ref int damageAmount, ref HazardType hazardType);
    }
}
