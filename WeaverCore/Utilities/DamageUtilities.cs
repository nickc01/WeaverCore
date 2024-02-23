using GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Enums;
using WeaverCore.Interfaces;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Contains utility functions related to damaging the player and enemies
    /// </summary>
    public static class DamageUtilities
    {
        class LocalPlayerDamageModifier : IGlobalPlayerDamageModifier
        {
            public void OnPlayerHit(HeroController __instance, ref GameObject go, ref CollisionSide damageSide, ref int damageAmount, ref HazardType hazardType)
            {
                foreach (var modifier in go.GetComponents<IPlayerDamageModifier>())
                {
                    try
                    {
                        modifier.OnPlayerHit(__instance, ref damageSide, ref damageAmount, ref hazardType);
                    }
                    catch (Exception e)
                    {
                        WeaverLog.LogError($"Error running player damage modifier {modifier?.GetType().FullName ?? "null"}");
                        WeaverLog.LogException(e);
                    }
                }
            }
        }

        class LocalEnemyDamageModifier : IGlobalEnemyDamageModifier
        {
            public void OnHit(GameObject attacker, GameObject go, ref AttackType attackType, ref int damage, ref float direction, ref bool ignoreInvincible, ref float attackStrength)
            {
                foreach (var modifier in go.GetComponents<IEnemyDamageModifier>())
                {
                    try
                    {
                        modifier.OnHit(attacker, ref attackType, ref damage, ref direction, ref ignoreInvincible, ref attackStrength);
                    }
                    catch (Exception e)
                    {
                        WeaverLog.LogError($"Error running enemy damage modifier {modifier?.GetType().FullName ?? "null"}");
                        WeaverLog.LogException(e);
                    }
                }
            }
        }

        class PrioritySorter<T> : IComparer<KeyValuePair<int, T>>
        {
            Comparer<float> floatComparer = Comparer<float>.Default;

            public int Compare(KeyValuePair<int, T> x, KeyValuePair<int, T> y)
            {
                return floatComparer.Compare(y.Key, x.Key);
            }
        }

        static SortedSet<KeyValuePair<int, IGlobalPlayerDamageModifier>> globalPlayerModifiers = new SortedSet<KeyValuePair<int, IGlobalPlayerDamageModifier>>(new PrioritySorter<IGlobalPlayerDamageModifier>());
        static SortedSet<KeyValuePair<int, IGlobalEnemyDamageModifier>> globalEnemyModifiers = new SortedSet<KeyValuePair<int, IGlobalEnemyDamageModifier>>(new PrioritySorter<IGlobalEnemyDamageModifier>());

        public static IEnumerable<IGlobalPlayerDamageModifier> GlobalPlayerModifiers => globalPlayerModifiers.Select(kv => kv.Value);
        public static IEnumerable<IGlobalEnemyDamageModifier> GlobalEnemyModifiers => globalEnemyModifiers.Select(kv => kv.Value);

        public static bool AddGlobalPlayerModifier(IGlobalPlayerDamageModifier playerModifier, int priority = 0)
        {
            if (playerModifier == null)
            {
                throw new ArgumentNullException(nameof(playerModifier));
            }

            if (globalPlayerModifiers.Any(kv => kv.Value == playerModifier))
            {
                return false;
            }
            else
            {
                globalPlayerModifiers.Add(new KeyValuePair<int, IGlobalPlayerDamageModifier>(priority, playerModifier));
                return true;
            }
        }

        public static bool RemoveGlobalPlayerModifier(IGlobalPlayerDamageModifier playerModifier)
        {
            if (playerModifier == null)
            {
                throw new ArgumentNullException(nameof(playerModifier));
            }

            return globalPlayerModifiers.RemoveWhere(kv => kv.Value == playerModifier) > 0;

            //return globalPlayerModifiers.Remove(playerModifier);
        }

        public static bool AddGlobalEnemyModifier(IGlobalEnemyDamageModifier enemyModifier, int priority = 0)
        {
            if (enemyModifier == null)
            {
                throw new ArgumentNullException(nameof(enemyModifier));
            }

            if (globalEnemyModifiers.Any(kv => kv.Value == enemyModifier))
            {
                return false;
            }
            else
            {
                globalEnemyModifiers.Add(new KeyValuePair<int, IGlobalEnemyDamageModifier>(priority, enemyModifier));
                return true;
            }

            //return globalEnemyModifiers.Add(enemyModifier);
        }

        public static bool RemoveGlobalEnemyModifier(IGlobalEnemyDamageModifier enemyModifier)
        {
            if (enemyModifier == null)
            {
                throw new ArgumentNullException(nameof(enemyModifier));
            }

            return globalEnemyModifiers.RemoveWhere(kv => kv.Value == enemyModifier) > 0;

            //return globalEnemyModifiers.Remove(enemyModifier);
        }

        [OnHarmonyPatch]
        static void OnPatch(HarmonyPatcher patcher)
        {
            var orig = typeof(HeroController).GetMethod("TakeDamage", BindingFlags.Public | BindingFlags.Instance);
            var prefix = typeof(DamageUtilities).GetMethod(nameof(TakeDamagePrefix), BindingFlags.NonPublic | BindingFlags.Static);

            patcher.Patch(orig, prefix, null);

            AddGlobalPlayerModifier(new LocalPlayerDamageModifier(), -9999);
            AddGlobalEnemyModifier(new LocalEnemyDamageModifier(), -9999);
        }

        static bool TakeDamagePrefix(HeroController __instance, ref GameObject go, ref CollisionSide damageSide, ref int damageAmount, ref int hazardType)
        {
            ApplyPlayerDamageModifier(__instance, ref go, ref damageSide, ref damageAmount, ref hazardType);
            return true;
        }

        public static void ApplyPlayerDamageModifier(HeroController __instance, ref GameObject go, ref CollisionSide damageSide, ref int damageAmount, ref int hazardType)
        {
            foreach (var modifier in GlobalPlayerModifiers)
            {
                try
                {
                    HazardType damageType = (HazardType)hazardType;
                    modifier.OnPlayerHit(__instance, ref go, ref damageSide, ref damageAmount, ref damageType);
                    hazardType = (int)damageType;
                }
                catch (Exception e)
                {
                    WeaverLog.LogError($"Error running global player damage modifier {modifier?.GetType().FullName ?? "null"}");
                    WeaverLog.LogException(e);
                }
            }
        }

        public static void ApplyEnemyDamageModifier(GameObject attacker, GameObject go, ref AttackType attackType, ref int damage, ref float direction, ref bool ignoreInvincible, ref float attackStrength)
        {
            foreach (var modifier in GlobalEnemyModifiers)
            {
                try
                {
                    modifier.OnHit(attacker, go, ref attackType, ref damage, ref direction, ref ignoreInvincible, ref attackStrength);
                }
                catch (Exception e)
                {
                    WeaverLog.LogError($"Error running global enemy damage modifier {modifier?.GetType().FullName ?? "null"}");
                    WeaverLog.LogException(e);
                }
            }
        }

        public static void ApplyEnemyDamageModifier(GameObject go, ref HitInfo info)
        {
            ApplyEnemyDamageModifier(info.Attacker, go, ref info.AttackType, ref info.Damage, ref info.Direction, ref info.IgnoreInvincible, ref info.AttackStrength);
        }
    }
}
