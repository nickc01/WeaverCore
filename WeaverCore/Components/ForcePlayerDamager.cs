using GlobalEnums;
using System;
using System.Reflection;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// This component damages the player, even if the player is shadow dashing or desolate diving
    /// </summary>
    public class ForcePlayerDamager : DamageHero
    {
        static Func<HeroController, bool> CanTakeDamage;
        static Action<HeroController> CancelDash;

        [SerializeField]
        [Tooltip("Should damage be triggered immediately upon contact with the player?")]
        bool immediateDamageOnContact = true;

        public new HazardType hazardType
        {
            get => (HazardType)base.hazardType;
            set => base.hazardType = (int)value;
        }

        /// <summary>
        /// Forces the player to take damage. Ignores shadow dash and desolate dive invincibility
        /// </summary>
        /// <param name="controller">The player to damage</param>
        /// <param name="source">The source of the damage</param>
        /// <param name="damageSide">Which direction the damage is coming from</param>
        /// <param name="damageAmount">The amount of damage to deal</param>
        /// <param name="hazardType">The type of damage to deal</param>
        public static void ForceDamage(HeroController controller, GameObject source, CollisionSide damageSide, int damageAmount, HazardType hazardType)
        {
            if (damageAmount > 0 && CanForceDamage(controller))
            {
                if (CancelDash == null)
                {
                    CancelDash = ReflectionUtilities.MethodToDelegate<Action<HeroController>>(typeof(HeroController).GetMethod("CancelDash", BindingFlags.Instance | BindingFlags.NonPublic));
                }

                controller.CancelParryInvuln();
                CancelDash(controller);

                if (controller.damageMode != DamageMode.FULL_DAMAGE)
                {
                    controller.SetDamageMode(DamageMode.FULL_DAMAGE);
                }

                controller.TakeDamage(source, damageSide, damageAmount, (int)hazardType);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (immediateDamageOnContact && (collision.CompareTag("Player") || collision.CompareTag("HeroBox")))
            {
                ForceDamage(HeroController.instance, gameObject, CollisionSide.bottom, damageDealt, hazardType);
            }
        }

        static bool CanForceDamage(HeroController controller)
        {
            if (CanTakeDamage == null)
            {
                CanTakeDamage = ReflectionUtilities.MethodToDelegate<Func<HeroController, bool>>(typeof(HeroController).GetMethod("CanTakeDamage", BindingFlags.Instance | BindingFlags.NonPublic));
            }

            return CanTakeDamage(controller);
        }
    }
}
