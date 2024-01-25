using UnityEngine;
using WeaverCore.Enums;

namespace WeaverCore.Interfaces
{
    /// <summary>
    /// Used to adjust the damage an enemy receives
    /// </summary>
    public interface IEnemyDamageModifier
    {
        /// <summary>
        /// Used to modify hits applied to an object (Anything that has an <seealso cref="IHittable"/> component applied)
        /// </summary>
        /// <param name="attacker">The attacker that is initiating the hit</param>
        /// <param name="attackType">The type of attack that is being done</param>
        /// <param name="damage">How much damage the attack is doing</param>
        /// <param name="direction">The direction in degrees the attack is coming from</param>
        /// <param name="ignoreInvincible">Hits the enemy, even if the enemy is supposed to be invincible</param>
        /// <param name="attackStrength">How strong the attack was. Used for enemy recoil</param>
        void OnHit(GameObject attacker, ref AttackType attackType, ref int damage, ref float direction, ref bool ignoreInvincible, ref float attackStrength);
    }
}
