using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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
			/*AttackTypes attackType = AttackTypes.Generic;

			switch (hit.AttackType)
			{
				case AttackType.Nail:
					attackType = AttackTypes.Nail;
					break;
				case AttackType.Generic:
					attackType = AttackTypes.Generic;
					break;
				case AttackType.Spell:
					attackType = AttackTypes.Spell;
					break;
				case AttackType.Acid:
					attackType = AttackTypes.Acid;
					break;
				case AttackType.Splatter:
					attackType = AttackTypes.Splatter;
					break;
				case AttackType.RuinsWater:
					attackType = AttackTypes.RuinsWater;
					break;
				case AttackType.SharpShadow:
					attackType = AttackTypes.SharpShadow;
					break;
				case AttackType.NailBeam:
					attackType = AttackTypes.NailBeam;
					break;
			}*/

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
