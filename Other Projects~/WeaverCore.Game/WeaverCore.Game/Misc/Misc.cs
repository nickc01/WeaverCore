using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.DataTypes;
using WeaverCore.Enums;

namespace WeaverCore.Game
{
	public static class Misc
	{
		public static HitInfo ConvertHitInstance(HitInstance hitInstance, Transform target = null)
		{
			int instanceAttackType = (int)hitInstance.AttackType;

			var attackType = (AttackType)instanceAttackType;

			return new HitInfo()
			{
				Attacker = hitInstance.Source,
				AttackStrength = hitInstance.MagnitudeMultiplier,
				AttackType = attackType,
				Damage = hitInstance.DamageDealt,
				Direction = hitInstance.GetActualDirection(target),
				IgnoreInvincible = hitInstance.IgnoreInvulnerable
			};
		}


		public static HitInstance ConvertHitInfo(HitInfo hit)
		{
			int infoAttackType = (int)hit.AttackType;

			var attackType = (AttackTypes)infoAttackType;

			return new HitInstance()
			{
				AttackType = attackType,
				CircleDirection = false,
				DamageDealt = hit.Damage,
				Direction = hit.Direction,
				IgnoreInvulnerable = hit.IgnoreInvincible,
				IsExtraDamage = false,
				MagnitudeMultiplier = hit.AttackStrength,
				MoveAngle = 0f,
				MoveDirection = false,
				Multiplier = 1,
				Source = hit.Attacker,
				SpecialType = SpecialTypes.None
			};
		}
	}
}
