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
		public AttackTypes attackType;

		/// <summary>
		/// In which direction is the attack going towards? Only used if <see cref="EnemyDamager.ForceHitDirection"/> is set to true
		/// </summary>
		public CardinalDirection hitDirection;

		/// <summary>
		/// If true, the hit direction will always be in a particular direction
		/// </summary>
		
		public bool ForcedHitDirection = false;

		public bool IsContinuous = true;

		public float ContinousHitRate = 0.2f;

		const int DEFAULT_RECURSION_DEPTH = 3;

		public UnityEvent<GameObject, float> OnHitObject;

		private readonly HashSet<Collider2D> collidingObjects = new HashSet<Collider2D>();
        private Coroutine continuousDamageCoroutine;

		void OnTriggerEnter2D(Collider2D collider)
        {
            collidingObjects.Add(collider);
            ApplyDamage(collider);
        }

        void OnTriggerExit2D(Collider2D collider)
        {
            collidingObjects.Remove(collider);
        }

		/*void OnTriggerEnter2D(Collider2D collider)
		{
			var obj = collider.transform;

			var hitVector = (collider.gameObject.transform.position - transform.position).normalized;

			Debug.DrawRay(transform.position, hitVector * 3f, Color.Lerp(Color.red, Color.yellow, 0.5f), 10f);

			var angle = DirectionUtilities.ToDegrees(DirectionUtilities.RadToDirection(Mathf.Atan2(hitVector.y, hitVector.x)));

			Debug.DrawRay(transform.position, VectorUtilities.DegreesToVector(angle, 2f), Color.cyan, 10f);


			//var hits = HitEnemy(obj,gameObject,damage,attackType,DirectionUtilities.RadToDirection(Mathf.Atan2(hitVector.y, hitVector.x)));
			var hits = HitEnemy(obj,gameObject,damage,attackType, VectorUtilities.VectorToDegrees(hitVector));

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
		}*/

		void OnEnable()
        {
            if (IsContinuous)
            {
                continuousDamageCoroutine = StartCoroutine(ApplyContinuousDamage());
            }
        }

        void OnDisable()
        {
            if (continuousDamageCoroutine != null)
            {
                StopCoroutine(continuousDamageCoroutine);
            }
            collidingObjects.Clear();
        }

		IEnumerator ApplyContinuousDamage()
        {
            while (true)
            {
                yield return new WaitForSeconds(ContinousHitRate);

                foreach (var collider in collidingObjects)
                {
                    if (collider != null)
                    {
                        ApplyDamage(collider);
                    }
					else
					{
						collidingObjects.Remove(collider);
					}
                }
            }
        }

		private void ApplyDamage(Collider2D collider)
        {
            var obj = collider.transform;
            var hitVector = (collider.transform.position - transform.position).normalized;
            var angle = VectorUtilities.VectorToDegrees(hitVector);
			
			WeaverLog.Log("Attemping Damage to Object = " + obj);
            var hits = EnemyHealthUtilities.DealDamage(obj, gameObject, damage, attackType, angle);
			var extras = EnemyHealthUtilities.TriggerOtherHittables(obj, gameObject, damage, attackType, angle);
            if (attackType == AttackTypes.Acid)
            {
                EventManager.SendEventToGameObject("ACID", collider.gameObject, gameObject);
            }

            foreach (var hit in hits)
            {
                OnHitObject.Invoke(hit.gameObject, damage);
            }

			foreach (var hit in extras)
            {
				if (hit.SourceObj is Component c)
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
        /*public static System.Collections.Generic.List<IHittable> HitEnemy(Transform obj, GameObject attacker, int damage, AttackType type, CardinalDirection hitDirection)
        {
			return HitEnemy(obj, attacker, damage, type, hitDirection.ToDegrees());
        }

		public static System.Collections.Generic.List<IHittable> HitEnemy(Transform obj, GameObject attacker, int damage, AttackType type, float hitDirectionDegrees)
        {
            System.Collections.Generic.List<IHittable> hitObjects = new System.Collections.Generic.List<IHittable>();

            int depth = 0;

			while (obj != null)
			{
				var hittables = obj.GetComponents<IHittable>();
				if (hittables != null && hittables.Length > 0)
				{
					foreach (var hittable in hittables)
					{
						var hitInfo = new HitInfo()
						{
							Attacker = attacker,
							Damage = damage,
							AttackStrength = 1f,
							AttackType = type,
							Direction = hitDirectionDegrees,
							IgnoreInvincible = false
						};

						EnemyHealthUtilities.ApplyEnemyDamageModifier(obj.gameObject, ref hitInfo);

                        hittable.Hit(hitInfo);
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
        }*/
	}
}
