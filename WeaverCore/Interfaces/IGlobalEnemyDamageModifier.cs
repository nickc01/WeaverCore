using UnityEngine;
using WeaverCore.Enums;

namespace WeaverCore.Interfaces
{
    /// <summary>
    /// Used to adjust the enemy damage across all <see cref="IHittable"/>s
    /// </summary>
    public interface IGlobalEnemyDamageModifier
    {
        /// <summary>
        /// Used to modify hits applied to any enemy (Anything that has an <seealso cref="IHittable"/> component applied)
        /// </summary>
        /// <param name="attacker">The attacker that is initiating the hit</param>
        /// <param name="go">The object that is being hit</param>
        /// <param name="attackType">The type of attack that is being done</param>
        /// <param name="damage">How much damage the attack is doing</param>
        /// <param name="direction">The direction in degrees the attack is coming from</param>
        /// <param name="ignoreInvincible">Hits the enemy, even if the enemy is supposed to be invincible</param>
        /// <param name="attackStrength">How strong the attack was. Used for enemy recoil</param>
        void OnHit(GameObject attacker, GameObject go, ref AttackType attackType, ref int damage, ref float direction, ref bool ignoreInvincible, ref float attackStrength);
    }
}
