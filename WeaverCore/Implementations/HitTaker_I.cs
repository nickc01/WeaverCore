using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Implementations
{
    public abstract class HitTaker_I : IImplementation
    {
        public abstract bool Hit(Transform target, GameObject attacker, int damage, AttackType type, CardinalDirection hitDirection);

        public bool HitDefault(Transform target, GameObject attacker, int damage, AttackType type, CardinalDirection hitDirection)
        {
			bool damageDealt = false;
			while (target != null)
			{
				IHittable hittable = target.GetComponent<IHittable>();
				if (hittable != null)
				{
                    var hitInfo = new HitInfo()
                    {
                        Attacker = attacker,
                        Damage = damage,
                        AttackStrength = 1f,
                        AttackType = type,
                        Direction = hitDirection.ToDegrees(),
                        IgnoreInvincible = false
                    };

                    DamageUtilities.ApplyEnemyDamageModifier(target.gameObject, ref hitInfo);

                    if (hittable.Hit(hitInfo))
                    {
						damageDealt = true;
					}
				}
				target = target.parent;
			}
			return damageDealt;
		}
    }
}
