using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
    /// <summary>
    /// Applies extra damage to an enemy on Trigger (such as Spore damage or Dung damage)
    /// </summary>
    public class EnemyExtraDamager : MonoBehaviour 
	{
        /// <summary>
        /// The type of damage to deal
        /// </summary>
        public ExtraDamageTypes damageType;
        
        /// <summary>
        /// The interval in seconds between applying damage
        /// </summary>
        [Tooltip("The interval in seconds between applying damage")]
        [SerializeField] private float damageRate = 0.8f;

        private Dictionary<Collider2D, float> lastDamageTimeByCollider = new Dictionary<Collider2D, float>();

        public const int DEFAULT_RECURSION_DEPTH = 3;

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            ApplyDamageToCollider(collider);
        }
        
        protected virtual void OnTriggerStay2D(Collider2D collider)
        {
            ApplyDamageToCollider(collider);
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            ApplyDamageToCollider(collision.collider);
        }
        
        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            ApplyDamageToCollider(collision.collider);
        }
        
        private void ApplyDamageToCollider(Collider2D collider)
        {
            if (collider == null) return;
            
            float currentTime = Time.time;
            
            // Check if we should apply damage based on the rate
            if (!lastDamageTimeByCollider.TryGetValue(collider, out float lastDamageTime) || 
                currentTime - lastDamageTime >= damageRate)
            {
                var obj = collider.transform;

                WeaverLog.Log("HIT ENEMY = " + obj.name);
                
                // Use EnemyHealthUtilities to handle extra damagables
                var extraDamageables = EnemyHealthUtilities.GetHealthComponentsInParent(obj);
                bool hitSomething = false;
                
                foreach (var healthWrapper in extraDamageables)
                {
                    if (healthWrapper.HealthComponent.GetComponent<IExtraDamageable>() is IExtraDamageable extraDamageable)
                    {
                        extraDamageable.RecieveExtraDamage(damageType);
                        OnExtraDamage(extraDamageable);
                        hitSomething = true;
                    }
                }
                
                // If no health components were found with IExtraDamageable, fall back to the old method
                if (!hitSomething && HitEnemy(obj, damageType, OnExtraDamage).Count == 0)
                {
                    OnDamageBackup(obj);
                }
                
                // Update the last damage time for this collider
                lastDamageTimeByCollider[collider] = currentTime;
            }
        }

        /// <summary>
        /// Called when an enemy was sucessfully hit
        /// </summary>
        /// <param name="hitEnemy">The enemy that was hit</param>
        protected virtual void OnExtraDamage(IExtraDamageable hitEnemy)
        {

        }

        /// <summary>
        /// If a hit enemy doesn't have an IExtraDamagable component, this is used as a backup
        /// </summary>
        protected virtual void OnDamageBackup(Transform obj)
        {

        }


        /// <summary>
        /// Applies extra damage to an enemy (such as Spore damage or Dung damage)
        /// </summary>
        /// <param name="obj">The transform of the enemy to hit</param>
        /// <param name="damageType">The type of damage to deal</param>
        /// <param name="onHit">Called when the enemy was sucessfully hit</param>
        public static System.Collections.Generic.List<IExtraDamageable> HitEnemy(Transform obj, ExtraDamageTypes damageType, Action<IExtraDamageable> onHit = null)
        {
            WeaverLog.Log("HIT ENEMY = " + obj);
            System.Collections.Generic.List<IExtraDamageable> hitEnemies = new System.Collections.Generic.List<IExtraDamageable>();
            int depth = 0;

            while (obj != null)
            {
                IExtraDamageable hittable = obj.GetComponent<IExtraDamageable>();
                if (hittable != null)
                {
                    hitEnemies.Add(hittable);
                    hittable.RecieveExtraDamage(damageType);
                    onHit?.Invoke(hittable);
                }
                obj = obj.parent;
                depth += 1;
                if (depth == DEFAULT_RECURSION_DEPTH)
                {
                    break;
                }
            }

            return hitEnemies;
        }
    }
}
