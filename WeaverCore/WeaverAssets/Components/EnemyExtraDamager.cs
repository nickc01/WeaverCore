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

        public const int DEFAULT_RECURSION_DEPTH = 3;

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            var obj = collider.transform;
            if (HitEnemy(obj, damageType, OnExtraDamage).Count == 0)
            {
                OnDamageBackup(obj);
            }
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            var obj = collision.collider.transform;
            if (HitEnemy(obj, damageType, OnExtraDamage).Count == 0)
            {
                OnDamageBackup(obj);
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
                depth += DEFAULT_RECURSION_DEPTH;
                if (depth == DEFAULT_RECURSION_DEPTH)
                {
                    break;
                }
            }

            return hitEnemies;
        }
    }
}
