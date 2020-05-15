using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore
{
	public enum CardinalDirection
	{
		Up,
		Down,
		Left,
		Right
	}

	/// <summary>
	/// Info on how something is hitting something else
	/// </summary>
	public struct HitInfo
	{
		/// <summary>
		/// The attacker that is initiating the hit
		/// </summary>
		public GameObject Attacker;
		/// <summary>
		/// The type of attack that is being done
		/// </summary>
		public AttackType AttackType;
		/// <summary>
		/// How much damage the attack is doing
		/// </summary>
		public int Damage;
		/// <summary>
		/// The direction in degrees the attack is coming from
		/// </summary>
		public float Direction;

		/// <summary>
		/// Hits the enemy, even if the enemy is supposed to be invincible
		/// </summary>
		public bool IgnoreInvincible;

		/// <summary>
		/// How strong the attack was. Used for enemy recoil
		/// </summary>
		public float AttackStrength;
	}

	public static class DirectionFunctions
	{
		public static float ToDegrees(this CardinalDirection direction)
		{
			switch (direction)
			{
				case CardinalDirection.Up:
					return 90f;
				case CardinalDirection.Down:
					return 270f;
				case CardinalDirection.Left:
					return 180f;
				case CardinalDirection.Right:
					return 0f;
				default:
					return 0f;
			}
		}

		public static float ToRads(this CardinalDirection direction)
		{
			return ToDegrees(direction) * Mathf.Deg2Rad;
		}

		public static CardinalDirection RadToDirection(float rads)
		{
			return DegreesToDirection(rads * Mathf.Rad2Deg);
		}

		public static CardinalDirection DegreesToDirection(float degrees)
		{
			degrees %= 360;
			if (degrees < 0)
			{
				degrees += 360;
			}

			if (degrees > 45 && degrees <= 135)
			{
				return CardinalDirection.Up;
			}
			else if (degrees > 135 && degrees <= 225)
			{
				return CardinalDirection.Left;
			}
			else if (degrees > 225 && degrees <= 315)
			{
				return CardinalDirection.Down;
			}
			else
			{
				return CardinalDirection.Right;
			}
		}
	}

	/// <summary>
	/// What type of attack is being dealt
	/// </summary>
	public enum AttackType
	{
		Nail,
		Generic,
		Spell,
		Acid,
		Splatter,
		RuinsWater,
		SharpShadow,
		NailBeam
	}
}


namespace WeaverCore.Interfaces
{
	public interface IHitReceiver
	{
		void ReceiveHit(HitInfo hit);
	}
}
