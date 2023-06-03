using System;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
    public class EnemyExtraDamager : MonoBehaviour 
	{
        public ExtraDamageTypes damageType;

        public const int DEFAULT_RECURSION_DEPTH = 3;

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            var obj = collider.transform;
            HitEnemy(obj, damageType, OnExtraDamage);
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            var obj = collision.collider.transform;
            HitEnemy(obj, damageType, OnExtraDamage);
        }

        protected virtual void OnExtraDamage(IExtraDamageable hitEnemy)
        {

        }

        public static void HitEnemy(Transform obj, ExtraDamageTypes damageType, Action<IExtraDamageable> onHit = null)
        {
            int depth = 0;

            while (obj != null)
            {
                IExtraDamageable hittable = obj.GetComponent<IExtraDamageable>();
                if (hittable != null)
                {

                    hittable.RecieveExtraDamage(damageType);
                    onHit?.Invoke(hittable);


                    /*PlayMakerFSM playMakerFSM = FSMUtility.LocateFSM(enemyList[i], "Extra Damage");
                    if (playMakerFSM != null)
                    {
                        playMakerFSM.SendEvent(damageEvent);
                    }
                    enemyList[i].GetComponent<IExtraDamageable>()?.RecieveExtraDamage(extraDamageType);*/

                    /*hittable.Hit(new HitInfo()
                    {
                        Attacker = attacker,
                        Damage = damage,
                        AttackStrength = 1f,
                        AttackType = type,
                        Direction = hitDirection.ToDegrees(),
                        IgnoreInvincible = false
                    });*/
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
