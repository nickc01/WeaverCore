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

        public new HazardType hazardType
        {
            get => (HazardType)base.hazardType;
            set => base.hazardType = (int)value;
        }


        public static void ForceDamage(HeroController controller, GameObject source, CollisionSide damageSide, int damageAmount, HazardType hazardType)
        {
            if (damageAmount > 0 && CanForceDamage(controller))
            {
                if (CancelDash == null)
                {
                    CancelDash = ReflectionUtilities.MethodToDelegate<Action<HeroController>>(typeof(HeroController).GetMethod("CancelDash", BindingFlags.Instance | BindingFlags.NonPublic));
                }

                //controller.cState.shadowDashing = false;

                controller.CancelParryInvuln();
                CancelDash(controller);

                if (controller.damageMode != DamageMode.FULL_DAMAGE)
                {
                    controller.SetDamageMode(DamageMode.FULL_DAMAGE);
                }

                controller.TakeDamage(source, damageSide, damageAmount, (int)hazardType);
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
