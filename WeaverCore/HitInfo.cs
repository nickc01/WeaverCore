using System.Globalization;
using UnityEngine;
using WeaverCore.Enums;

namespace WeaverCore
{
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

		public override string ToString()
		{
			string final = "{ ";

			final = string.Concat(final, "Attacker = " + Attacker, ", ");
			final = string.Concat(final, "AttackType = " + AttackType, ", ");
			final = string.Concat(final, "Damage = " + Damage, ", ");
			final = string.Concat(final, "Direction = " + Direction, ", ");
			final = string.Concat(final, "IgnoreInvincible = " + IgnoreInvincible, ", ");
			final = string.Concat(final, "AttackStrength = " + AttackStrength, "}");
			return final;
		}

		/// <summary>
		/// Returns the player that is attacking. Returns null if the attacker is not a player
		/// </summary>
		public Player GetAttackingPlayer()
		{
			var playerLayer = LayerMask.NameToLayer("Player");
			var parent = Attacker.transform;

			while (parent != null)
			{
				if ((parent.gameObject.layer | playerLayer) == playerLayer)
				{
					var playerComponent = parent.gameObject.GetComponent<Player>();
					if (playerComponent != null)
					{
						return playerComponent;
					}
				}
				parent = parent.parent;
			}
			return null;
		}
	}
}
