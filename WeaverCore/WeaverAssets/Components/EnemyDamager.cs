using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{


    /// <summary>
    /// Used to damage an <see cref="WeaverCore.Features.Enemy"/> on contact
    /// </summary>
    public class EnemyDamager : MonoBehaviour
	{
		/// <summary>
		/// The amount of damage this will deal to the enemy
		/// </summary>
		public int damage = 32;

		/// <summary>
		/// The type of attack on the enemy
		/// </summary>
		public AttackType attackType;

		/// <summary>
		/// In which direction is the attack going towards?
		/// </summary>
		//[HideInInspector]
		public CardinalDirection hitDirection;

		const int DEFAULT_RECURSION_DEPTH = 3;

		void OnTriggerEnter2D(Collider2D collider)
		{
			var obj = collider.transform;

			HitEnemy(obj,gameObject,damage,attackType,hitDirection);
		}

		public static void HitEnemy(Transform obj, GameObject attacker, int damage, AttackType type, CardinalDirection hitDirection)
        {
			int depth = 0;

			while (obj != null)
			{
				IHittable hittable = obj.GetComponent<IHittable>();
				if (hittable != null)
				{
					hittable.Hit(new HitInfo()
					{
						Attacker = attacker,
						Damage = damage,
						AttackStrength = 1f,
						AttackType = type,
						Direction = hitDirection.ToDegrees(),
						IgnoreInvincible = false
					});
				}
				obj = obj.parent;
				depth += DEFAULT_RECURSION_DEPTH;
                if (depth == DEFAULT_RECURSION_DEPTH)
                {
					break;
                }
			}
		}
	}
}
