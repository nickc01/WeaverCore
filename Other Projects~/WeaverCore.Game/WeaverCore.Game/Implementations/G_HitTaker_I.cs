using Modding;
using System;
using System.Reflection;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Implementations
{
    public class G_HitTaker_I : HitTaker_I
    {
        public override bool Hit(Transform target, GameObject attacker, int damage, AttackType type, CardinalDirection hitDirection)
        {
            if (!HitDefault(target,attacker,damage,type,hitDirection))
            {
				bool damageDealt = false;
				var hit = Misc.ConvertHitInfo(new HitInfo
				{
					Attacker = attacker,
					Damage = damage,
					Direction = DirectionUtilities.ToDegrees(hitDirection),
					AttackType = type,
					AttackStrength = 1f,
					IgnoreInvincible = false
				});

				Transform transform = target;
				for (int i = 0; i < 3; i++)
				{
					IHitResponder component = transform.GetComponent<IHitResponder>();
					if (component != null)
					{
						damageDealt = true;
						component.Hit(hit);
					}
					transform = transform.parent;
					if (transform == null)
					{
						break;
					}
				}
				return damageDealt;
			}
			return false;
        }
    }
}
