using System;
using System.Collections;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Dreamnail
{


    /// <summary>
    /// A basic component that gives the player soul when dreamnailed
    /// </summary>
    public class DreamnailableObject : MonoBehaviour, IDreamnailable
    {
        /// <summary>
        /// Can the object be dreamnailed? 
        /// </summary>
        public bool CanBeDreamnailed => Time.time >= LastDreamnailHitTime + cooldownDuration && enabled && gameObject.activeInHierarchy;

        [SerializeField]
        [Tooltip("Specifies how much soul the player gains upon dreamnailing")]
        protected int soulAmount = 33;

        [SerializeField]
        [Tooltip("The cooldown before this object can be dreamnailed again. Set to 0 for no cooldown")]
        protected float cooldownDuration = 0.2f;

        [SerializeField]
        [Tooltip("Should the object be recoiled when dreamnailed?")]
        protected bool recoilWhenHit = false;

        [SerializeField]
        [Tooltip("Should the object flash when dreamnailed?")]
        protected bool flashWhenHit = true;

        /// <summary>
        /// The time the object was hit last (or 0 if the object was never hit).
        /// </summary>
        public float LastDreamnailHitTime { get; protected set; } = 0;

        public int DreamnailHit(Player player)
        {
            if (!CanBeDreamnailed)
            {
                return 0;
            }
            var amount = OnDreamnailHit(player);
            if (PlayerData.instance.GetBool("equippedCharm_30"))
            {
                amount *= 2;
            }

            if (recoilWhenHit)
            {
                Recoil recoiler = GetComponent<Recoil>();
                if (recoiler != null)
                {
                    bool flag = HeroController.instance.transform.localScale.x <= 0f;
                    recoiler.RecoilByDirection((!flag) ? 2 : 0, 2f);
                }
            }

            if (flashWhenHit)
            {
                foreach (var flasher in gameObject.GetComponentsInChildren<SpriteFlasher>())
                {
                    if (flasher != null)
                    {
                        flasher.flashDreamImpact();
                    }
                }
                /*SpriteFlasher flasher = gameObject.GetComponent<SpriteFlasher>();
                if (flasher != null)
                {
                    flasher.flashDreamImpact();
                }*/
            }

            LastDreamnailHitTime = Time.time;

            return amount;
        }

        protected virtual int OnDreamnailHit(Player player)
        {
            return soulAmount;
        }

        protected virtual void Reset()
        {
            gameObject.layer = 19;
        }
    }
}
