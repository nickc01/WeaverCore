using Mono.Collections.Generic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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

		public UnityEvent<GameObject, float> OnHitObject;

		void OnTriggerEnter2D(Collider2D collider)
		{
			var obj = collider.transform;

			var hits = HitEnemy(obj,gameObject,damage,attackType,hitDirection);

			if (attackType == AttackType.Acid)
			{
				EventManager.SendEventToGameObject("ACID", collider.gameObject, gameObject);
			}

			foreach (var hit in hits)
			{
				if (hit is Component c)
				{
                    OnHitObject.Invoke(c.gameObject, damage);
                }
            }
		}

        /// <summary>
        /// Hits a specified enemy
        /// </summary>
        /// <param name="obj">The transform of the enemy to hit</param>
        /// <param name="attacker">The GameObject initiating the attack.</param>
        /// <param name="damage">The amount of damage to be dealt.</param>
        /// <param name="type">The type of attack.</param>
        /// <param name="hitDirection">The cardinal direction of the attack.</param>
        /// <returns>A list of IHittable objects that were successfully hit.</returns>
        public static List<IHittable> HitEnemy(Transform obj, GameObject attacker, int damage, AttackType type, CardinalDirection hitDirection)
        {
            List<IHittable> hitObjects = new List<IHittable>();

            int depth = 0;

			while (obj != null)
			{
				var hittables = obj.GetComponents<IHittable>();
				if (hittables != null && hittables.Length > 0)
				{
					foreach (var hittable in hittables)
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
                        hitObjects.Add(hittable);
                    }
                }
				obj = obj.parent;
				depth += 1;
                if (depth == DEFAULT_RECURSION_DEPTH)
                {
					break;
                }
			}
			return hitObjects;
        }
	}
}
