using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace WeaverCore.Components
{
    /// <summary>
    /// The box that is used to damage the player if the player doesn't have acid armor on
    /// </summary>
    public class WeaverAcidBox : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The surface water objects of the acid. These will get disabled if the player doesn't have isma's tear. These must be disabled in that case, or else glitchy behaviour occurs")]
        List<WeaverSurfaceWater> surfaceWater;

        [SerializeField]
        [Tooltip("The colliders that will damage the player upon contact. Any boxes added to this list will get disabled when isma's tear is equipped")]
        List<Collider2D> acidDamageBoxes;

        readonly Dictionary<HeroController, int> touchingHeroCounts = new Dictionary<HeroController, int>();

        Collider2D acidTrigger;
        bool acidArmorUnlocked;

        static MethodInfo enterAcidMethod;
        static MethodInfo exitAcidMethod;

        private void Awake()
        {
            acidTrigger = GetComponent<Collider2D>();
            StartCoroutine(ArmorCheck());
        }

        private void OnDisable()
        {
            ClearTrackedHeroes();
        }

        IEnumerator ArmorCheck()
        {
            yield return null;

            if (surfaceWater != null)
            {
                foreach (var water in surfaceWater)
                {
                    if (water != null)
                    {
                        water.enabled = false;
                        water.GetComponent<Collider2D>().enabled = false;
                    }
                }
            }

            if (acidDamageBoxes != null)
            {
                foreach (var box in acidDamageBoxes)
                {
                    if (box != null)
                    {
                        box.enabled = true;
                    }
                }
            }
            while (PlayerData.instance == null || !PlayerData.instance.GetBool("hasAcidArmour"))
            {
                yield return new WaitForSeconds(0.1f);
            }

            acidArmorUnlocked = true;

            foreach (var damager in GetComponentsInChildren<PlayerDamager>())
            {
                damager.damageDealt = 0;
            }

            if (surfaceWater != null)
            {
                foreach (var water in surfaceWater)
                {
                    if (water != null)
                    {
                        water.enabled = true;
                        water.GetComponent<Collider2D>().enabled = true;
                    }
                }
            }

            if (acidDamageBoxes != null)
            {
                foreach (var box in acidDamageBoxes)
                {
                    if (box != null)
                    {
                        box.enabled = false;
                    }
                }
            }

            SyncCurrentPlayerOverlaps();
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (!acidArmorUnlocked || !TryGetHero(collision, out var hero))
            {
                return;
            }

            if (!touchingHeroCounts.TryGetValue(hero, out var count))
            {
                count = 0;
            }

            count++;
            touchingHeroCounts[hero] = count;

            if (count == 1)
            {
                SetHeroAcidState(hero, true);
            }
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (!TryGetHero(collision, out var hero))
            {
                return;
            }

            if (!touchingHeroCounts.TryGetValue(hero, out var count))
            {
                return;
            }

            count--;
            if (count <= 0)
            {
                touchingHeroCounts.Remove(hero);
                SetHeroAcidState(hero, false);
            }
            else
            {
                touchingHeroCounts[hero] = count;
            }
        }

        bool TryGetHero(Collider2D collider, out HeroController hero)
        {
            hero = null;
            if (collider == null)
            {
                return false;
            }

            hero = collider.GetComponentInParent<HeroController>();
            return hero != null;
        }

        void SyncCurrentPlayerOverlaps()
        {
            if (acidTrigger == null)
            {
                return;
            }

            Bounds bounds = acidTrigger.bounds;
            var overlaps = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0f);
            foreach (var overlap in overlaps)
            {
                if (overlap == null || overlap == acidTrigger)
                {
                    continue;
                }

                OnTriggerEnter2D(overlap);
            }
        }

        void ClearTrackedHeroes()
        {
            foreach (var kv in touchingHeroCounts)
            {
                SetHeroAcidState(kv.Key, false);
            }
            touchingHeroCounts.Clear();
        }

        static void EnsureAcidMethodCache()
        {
            if (enterAcidMethod != null && exitAcidMethod != null)
            {
                return;
            }

            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            enterAcidMethod = typeof(HeroController).GetMethod("EnterAcid", flags);
            exitAcidMethod = typeof(HeroController).GetMethod("ExitAcid", flags);
        }

        static void SetHeroAcidState(HeroController hero, bool inAcid)
        {
            if (hero == null)
            {
                return;
            }

            EnsureAcidMethodCache();

            try
            {
                if (inAcid && enterAcidMethod != null)
                {
                    enterAcidMethod.Invoke(hero, null);
                    return;
                }

                if (!inAcid && exitAcidMethod != null)
                {
                    exitAcidMethod.Invoke(hero, null);
                    return;
                }
            }
            catch
            {
            }

            if (hero.TryGetComponent<Rigidbody2D>(out var rb2d))
            {
                rb2d.gravityScale = inAcid ? hero.UNDERWATER_GRAVITY : hero.DEFAULT_GRAVITY;
            }

            hero.inAcid = inAcid;
            hero.cState.inAcid = inAcid;
        }
    }
}
