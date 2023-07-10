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

			foreach (var hit in hits)
			{
				if (hit is Component c)
				{
                    OnHitObject.Invoke(c.gameObject, damage);
                }
            }
		}

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
				depth += DEFAULT_RECURSION_DEPTH;
                if (depth == DEFAULT_RECURSION_DEPTH)
                {
					break;
                }
			}
			return hitObjects;
        }
	}
}
