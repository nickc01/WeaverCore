using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GlobalEnums;
using UnityEngine;
using UnityEngine.SceneManagement;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Utilities
{
    public static class EnemyHealthUtilities
    {
        public static Type EntityHealthType = typeof(EntityHealth);
        public static Type HealthManagerType;
        public static Type IHittableType = typeof(IHittable);
        public static Type IHitResponderType;
        public static Type HitInstanceType;
        
        // Dictionary to store death event handlers for HealthManager instances
        private static readonly Dictionary<MonoBehaviour, List<Action<HitInfo>>> healthManagerWrappers = new Dictionary<MonoBehaviour, List<Action<HitInfo>>>();

        const int DEFAULT_RECURSION_DEPTH = 3;

        [OnHarmonyPatch]
        static void OnPatch(HarmonyPatcher patcher)
        {
            {
                var orig = typeof(HeroController).GetMethod("TakeDamage", BindingFlags.Public | BindingFlags.Instance);
                var prefix = typeof(EnemyHealthUtilities).GetMethod(nameof(TakeDamagePrefix), BindingFlags.NonPublic | BindingFlags.Static);

                patcher.Patch(orig, prefix, null);
            }

            if (Initialization.Environment == RunningState.Game)
            {
                HealthManagerType = typeof(HeroController).Assembly.GetType("HealthManager");
                IHitResponderType = typeof(HeroController).Assembly.GetType("IHitResponder");
                HitInstanceType = typeof(HeroController).Assembly.GetType("HitInstance");

                var orig = HealthManagerType.GetMethod("Die");
                var prefix = typeof(EnemyHealthUtilities).GetMethod(nameof(HealthManagerDie_Prefix), BindingFlags.NonPublic | BindingFlags.Static);
                var postfix = typeof(EnemyHealthUtilities).GetMethod(nameof(HealthManagerDie_Postfix), BindingFlags.NonPublic | BindingFlags.Static);

                patcher.Patch(orig, prefix, postfix);
            }

            AddGlobalPlayerModifier(new LocalPlayerDamageModifier(), -9999);
            AddGlobalEnemyModifier(new LocalEnemyDamageModifier(), -9999);

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnActiveSceneChange;

            UnboundCoroutine.Start(PruneEveryMinute());
        }

        static IEnumerator PruneEveryMinute()
        {
            while (true)
            {
                yield return new WaitForSeconds(60f);
                PruneHealthManagerWrappers();
            }
        }

        static void OnActiveSceneChange(Scene prev, Scene current)
        {
            PruneHealthManagerWrappers();
        }

        static void PruneHealthManagerWrappers()
        {
            List<MonoBehaviour> keysToRemove = new List<MonoBehaviour>();

            foreach (var key in healthManagerWrappers.Keys)
            {
                if (key == null)
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var key in keysToRemove)
            {
                healthManagerWrappers.Remove(key);
            }
        }

        class PreviousDieState
        {
            public bool isDead;
        }

        static bool HealthManagerDie_Prefix(MonoBehaviour __instance, ref PreviousDieState __state)
        {
            if (__instance.GetType() == HealthManagerType)
            {
                __state = new PreviousDieState { isDead = __instance.ReflectGetField<bool>("isDead") };
            }
            else
            {
                __state = null;
            }
            return true;
        }

        static void HealthManagerDie_Postfix(MonoBehaviour __instance, PreviousDieState __state, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
        {
            if (__state != null && !__state.isDead && __instance.ReflectGetField<bool>("isDead"))
            {
                // Get the associated wrapper if it exists
                if (healthManagerWrappers.TryGetValue(__instance, out var onDeathEvents) && onDeathEvents.Count > 0)
                {
                    // Create hit info from the parameters
                    var hitInfo = new HitInfo
                    {
                        Direction = attackDirection ?? 0f,
                        AttackType = attackType,
                        IgnoreInvincible = ignoreEvasion
                    };

                    // Trigger all registered death events
                    foreach (var callback in onDeathEvents.ToArray())
                    {
                        try
                        {
                            callback?.Invoke(hitInfo);
                        }
                        catch (Exception e)
                        {
                            WeaverLog.LogError($"Error triggering OnDeath event for {__instance}");
                            WeaverLog.LogException(e);
                        }
                    }
                }
            }
        }

        static bool TakeDamagePrefix(HeroController __instance, ref GameObject go, ref CollisionSide damageSide, ref int damageAmount, ref int hazardType)
        {
            ApplyPlayerDamageModifier(__instance, ref go, ref damageSide, ref damageAmount, ref hazardType);
            return true;
        }

        public class EntityHealthWrapper : HealthWrapper
        {
            public EntityHealth EntityHealthComponent { get; private set; }

            public EntityHealthWrapper(EntityHealth entityHealth)
            {
                EntityHealthComponent = entityHealth;
            }

            public override MonoBehaviour HealthComponent { get => EntityHealthComponent; }

            public override int Health { get => EntityHealthComponent.Health; set => EntityHealthComponent.Health = value; }
            public override int SmallGeo { get => EntityHealthComponent.SmallGeo; set => EntityHealthComponent.SmallGeo = value; }
            public override int MediumGeo { get => EntityHealthComponent.MediumGeo; set => EntityHealthComponent.MediumGeo = value; }
            public override int LargeGeo { get => EntityHealthComponent.LargeGeo; set => EntityHealthComponent.LargeGeo = value; }
            public override bool Invincible { get => EntityHealthComponent.Invincible; set => EntityHealthComponent.Invincible = value; }

            public override bool IsDead => EntityHealthComponent.Health <= 0;

            public override event Action<HitInfo> OnDeath
            {
                add => EntityHealthComponent.OnDeathEvent += value;

                remove => EntityHealthComponent.OnDeathEvent -= value;
            }

            public override void Die(HitInfo hit) => EntityHealthComponent.Die(hit);

            public override bool Hit(HitInfo hit) => EntityHealthComponent.Hit(hit);
        }

        public class HealthManagerWrapper : HealthWrapper
        {
            MonoBehaviour healthComponent;

            public HealthManagerWrapper(MonoBehaviour healthComponent)
            {
                if (healthComponent?.GetType() != HealthManagerType)
                {
                    throw new Exception($"Component {healthComponent?.GetType().FullName ?? "null"} isn't a {HealthManagerType.Name}");
                }
                this.healthComponent = healthComponent;
            }

            public override MonoBehaviour HealthComponent => healthComponent;

            public override int Health { get => HealthComponent.ReflectGetField<int>("hp"); set => HealthComponent.ReflectSetField("hp", value); }
            public override int SmallGeo { get => HealthComponent.ReflectGetField<int>("smallGeoDrops"); set => HealthComponent.ReflectCallMethod("SetGeoSmall", CacheUtilities.GetTempSingleArray<object>(value)); }
            public override int MediumGeo { get => HealthComponent.ReflectGetField<int>("mediumGeoDrops"); set => HealthComponent.ReflectCallMethod("SetGeoMedium", CacheUtilities.GetTempSingleArray<object>(value));  }
            public override int LargeGeo { get => HealthComponent.ReflectGetField<int>("largeGeoDrops"); set => HealthComponent.ReflectCallMethod("SetGeoLarge", CacheUtilities.GetTempSingleArray<object>(value));  }
            public override bool Invincible { get => HealthComponent.ReflectGetField<bool>("invincible"); set => HealthComponent.ReflectSetField("invincible", value); }

            public override bool IsDead => HealthComponent.ReflectGetField<bool>("isDead");

            public override event Action<HitInfo> OnDeath
            {
                add
                {
                    if (!healthManagerWrappers.TryGetValue(HealthComponent, out var callbacks))
                    {
                        callbacks = new List<Action<HitInfo>>();
                        healthManagerWrappers[HealthComponent] = callbacks;
                    }
                    
                    if (!callbacks.Contains(value))
                    {
                        callbacks.Add(value);
                    }
                }

                remove
                {
                    if (healthManagerWrappers.TryGetValue(HealthComponent, out var callbacks))
                    {
                        callbacks.Remove(value);
                        
                        // Clean up empty lists
                        if (callbacks.Count == 0)
                        {
                            healthManagerWrappers.Remove(HealthComponent);
                        }
                    }
                }
            }

            public override void Die(HitInfo hit)
            {
                HealthComponent.ReflectCallMethod("Die", new object[]{ new float?(hit.Direction), hit.AttackType, hit.IgnoreInvincible });
            }

            public override bool Hit(HitInfo hit)
            {
                // Store the health before hit
                int healthBefore = Health;
                
                // Get HitInstance type through reflection
                if (HitInstanceType == null)
                {
                    WeaverLog.LogError("Could not find HitInstance type");
                    return false;
                }
                
                // Get AttackTypes enum type through reflection
                Type attackTypesEnumType = HealthManagerType.Assembly.GetType("AttackTypes");
                if (attackTypesEnumType == null)
                {
                    WeaverLog.LogError("Could not find AttackTypes enum type");
                    return false;
                }
                
                // Create a HitInstance through reflection
                object hitInstance = Activator.CreateInstance(HitInstanceType);
                
                // Set the fields using reflection
                hitInstance.ReflectSetField("Source", hit.Attacker);
                hitInstance.ReflectSetField("AttackType", hit.AttackType);
                hitInstance.ReflectSetField("DamageDealt", hit.Damage);
                hitInstance.ReflectSetField("Direction", hit.Direction);
                hitInstance.ReflectSetField("IgnoreInvulnerable", hit.IgnoreInvincible);
                hitInstance.ReflectSetField("MagnitudeMultiplier", 1.0f);
                hitInstance.ReflectSetField("Multiplier", hit.AttackStrength);

                HealthComponent.ReflectCallMethod("Hit", CacheUtilities.GetTempSingleArray(hitInstance));

                return Health < healthBefore;
            }
        }

        public class IHittableWrapper : ExtraHitWrapper
        {
            public IHittableWrapper(IHittable hittable)
            {
                HittableObj = hittable;
            }

            public IHittable HittableObj { get; private set; }
            public override object SourceObj => HittableObj;

            public override void Hit(HitInfo hit) => HittableObj.Hit(hit);
        }

        public class IHitResponderWrapper : ExtraHitWrapper
        {
            object _sourceObj;
            public IHitResponderWrapper(object sourceObj)
            {
                if (sourceObj.GetType() != IHitResponderType)
                {
                    throw new Exception($"Component {sourceObj?.GetType().FullName ?? "null"} isn't a {HealthManagerType.Name}");
                }
                _sourceObj = sourceObj;
            }

            public override object SourceObj => _sourceObj;

            public override void Hit(HitInfo hit)
            {
                // Get HitInstance type through reflection
                if (HitInstanceType == null)
                {
                    WeaverLog.LogError("Could not find HitInstance type");
                    return;
                }
                
                // Get AttackTypes enum type through reflection
                Type attackTypesEnumType = HealthManagerType.Assembly.GetType("AttackTypes");
                if (attackTypesEnumType == null)
                {
                    WeaverLog.LogError("Could not find AttackTypes enum type");
                    return;
                }
                
                // Create a HitInstance through reflection
                object hitInstance = Activator.CreateInstance(HitInstanceType);
                
                // Set the fields using reflection
                hitInstance.ReflectSetField("Source", hit.Attacker);
                hitInstance.ReflectSetField("AttackType", hit.AttackType);
                hitInstance.ReflectSetField("DamageDealt", hit.Damage);
                hitInstance.ReflectSetField("Direction", hit.Direction);
                hitInstance.ReflectSetField("IgnoreInvulnerable", hit.IgnoreInvincible);
                hitInstance.ReflectSetField("MagnitudeMultiplier", 1.0f);
                hitInstance.ReflectSetField("Multiplier", hit.AttackStrength);

                SourceObj.ReflectCallMethod("Hit", CacheUtilities.GetTempSingleArray(hitInstance));
            }
        }

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
            public void OnHit(GameObject attacker, GameObject go, ref AttackTypes attackType, ref int damage, ref float direction, ref bool ignoreInvincible, ref float attackStrength)
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

        public static void ApplyEnemyDamageModifier(GameObject attacker, GameObject go, ref AttackTypes attackType, ref int damage, ref float direction, ref bool ignoreInvincible, ref float attackStrength)
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

        /// <summary>
        /// Hits a specified enemy
        /// </summary>
        /// <param name="obj">The transform of the enemy to hit</param>
        /// <param name="attacker">The GameObject initiating the attack.</param>
        /// <param name="damage">The amount of damage to be dealt.</param>
        /// <param name="type">The type of attack.</param>
        /// <param name="hitDirection">The cardinal direction of the attack.</param>
        /// <returns>A list of IHittable objects that were successfully hit.</returns>
        public static List<HealthWrapper> DealDamage(Transform obj, GameObject attacker, int damage, AttackTypes type, CardinalDirection hitDirection)
        {
			return DealDamage(obj, attacker, damage, type, hitDirection.ToDegrees());
        }

		public static List<HealthWrapper> DealDamage(Transform obj, GameObject attacker, int damage, AttackTypes type, float hitDirectionDegrees)
        {
            return DealDamage(obj, new HitInfo {
                Attacker = attacker,
                Damage = damage,
                AttackStrength = 1f,
                AttackType = type,
                Direction = hitDirectionDegrees,
                IgnoreInvincible = false
            });
        }

        public static void FindAllEnemies(List<Component> enemies, bool includeInactive = false)
        {
            enemies.Clear();
            if (HealthManagerType != null)
            {
                enemies.AddRange(GameObject.FindObjectsOfType(HealthManagerType, includeInactive).OfType<Component>());
            }

            foreach (var et in GameObject.FindObjectsOfType(EntityHealthType, includeInactive).OfType<Component>())
            {
                if (!enemies.Any(e => e.gameObject == et.gameObject))
                {
                    enemies.Add(et);
                }
            }
        }

        public static HealthWrapper GetHealthComponent(this GameObject gameObject)
        {
            if (HealthManagerType != null && gameObject.TryGetComponent(HealthManagerType, out var c) && !c.GetType().Name.Contains("HealthManagerProxy"))
            {
                return new HealthManagerWrapper(c as MonoBehaviour);
            }
            if (gameObject.TryGetComponent<EntityHealth>(out var eh))
            {
                return new EntityHealthWrapper(eh);
            }

            return null;
        }

        public static HealthWrapper GetHealthComponent(this Component component)
        {
            if (HealthManagerType != null && component.TryGetComponent(HealthManagerType, out var c) && !c.GetType().Name.Contains("HealthManagerProxy"))
            {
                return new HealthManagerWrapper(c as MonoBehaviour);
            }
            if (component.TryGetComponent<EntityHealth>(out var eh))
            {
                return new EntityHealthWrapper(eh);
            }

            return null;
        }

        public static bool TryGetHealthComponent(this GameObject gameObject, out HealthWrapper healthComponent)
        {
            return (healthComponent = GetHealthComponent(gameObject)) != null;
        }

        public static bool TryGetHealthComponent(this Component component, out HealthWrapper healthComponent)
        {
            return (healthComponent = GetHealthComponent(component)) != null;
        }


        public static HealthWrapper GetHealthComponentInParent(this GameObject gameObject)
        {
            var transform = gameObject.transform;
            while (transform != null)
            {
                if (TryGetHealthComponent(transform, out var healthComponent))
                {
                    return healthComponent;
                }
                transform = transform.parent;
            }

            return null;
        }

        public static HealthWrapper GetHealthComponentInParent(this Component component)
        {
            return GetHealthComponentInParent(component);
        }

        public static bool TryGetHealthComponentInParent(this GameObject gameObject, out HealthWrapper healthComponent)
        {
            return (healthComponent = GetHealthComponentInParent(gameObject)) != null;
        }

        public static bool TryGetHealthComponentInParent(this Component component, out HealthWrapper healthComponent)
        {
            return (healthComponent = GetHealthComponentInParent(component)) != null;
        }



        public static List<HealthWrapper> GetHealthComponentsInParent(this GameObject gameObject)
        {
            List<HealthWrapper> components = new List<HealthWrapper>();
            GetHealthComponentsInParent(gameObject, components);
            return components;
        }

        public static List<HealthWrapper> GetHealthComponentsInParent(this Component c) => GetHealthComponentsInParent(c.gameObject);

        public static void GetHealthComponentsInParent(this GameObject gameObject, List<HealthWrapper> components)
        {
            components.Clear();
            var transform = gameObject.transform;
            while (transform != null)
            {
                if (TryGetHealthComponent(transform, out var healthComponent))
                {
                    components.Add(healthComponent);
                }
                transform = transform.parent;
            }
        }

        public static void GetHealthComponentsInParent(this Component c, List<HealthWrapper> components) => GetHealthComponentsInParent(c.gameObject, components);

        static List<Component> componentCache = new List<Component>();

        public static int GetOtherHittables(this GameObject gameObject, List<ExtraHitWrapper> hittables)
        {
            hittables.Clear();
            gameObject.GetComponents(IHittableType, componentCache);
            foreach (var c in componentCache)
            {
                if (!(c is EntityHealth) && c is IHittable ih)
                {
                    hittables.Add(new IHittableWrapper(ih));
                }
            }

            if (IHitResponderType != null)
            {
                gameObject.GetComponents(IHitResponderType, componentCache);
                foreach (var c in componentCache)
                {
                    if (!HealthManagerType.IsAssignableFrom(c.GetType()))
                    {
                        hittables.Add(new IHitResponderWrapper(c));
                    }
                }
            }

            return hittables.Count;
        }

        public static int GetOtherHittables(this Component component, List<ExtraHitWrapper> hittables)
        {
            return GetOtherHittables(component.gameObject, hittables);
        }

        public static IEnumerable<ExtraHitWrapper> GetOtherHittables(this GameObject gameObject)
        {
            gameObject.GetComponents(IHittableType, componentCache);
            foreach (var c in componentCache)
            {
                if (!(c is EntityHealth) && c is IHittable ih)
                {
                    yield return new IHittableWrapper(ih);
                }
            }

            if (IHitResponderType != null)
            {
                gameObject.GetComponents(IHitResponderType, componentCache);
                foreach (var c in componentCache)
                {
                    if (!HealthManagerType.IsAssignableFrom(c.GetType()))
                    {
                        yield return new IHitResponderWrapper(c);
                    }
                }
            }
        }

        public static IEnumerable<ExtraHitWrapper> GetOtherHittables(this Component component)
        {
            return GetOtherHittables(component.gameObject);
        }

        public static List<HealthWrapper> DealDamage(Transform obj, HitInfo hit)
        {
            List<HealthWrapper> hitObjects = new List<HealthWrapper>();

            int depth = 0;

			while (obj != null)
			{
				if (TryGetHealthComponent(obj, out var hittable))
				{
					var hitInfo = new HitInfo {
                        Attacker = hit.Attacker,
                        Damage = hit.Damage,
                        AttackStrength = hit.AttackStrength,
                        AttackType = hit.AttackType,
                        Direction = hit.Direction,
                        IgnoreInvincible = hit.IgnoreInvincible
                    };
                    ApplyEnemyDamageModifier(obj.gameObject, ref hitInfo);

                    hittable.Hit(hit);
                    hitObjects.Add(hittable);
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

        public static List<ExtraHitWrapper> TriggerOtherHittables(Transform obj, GameObject attacker, int damage, AttackTypes type, CardinalDirection hitDirection)
        {
			return TriggerOtherHittables(obj, attacker, damage, type, hitDirection.ToDegrees());
        }

		public static List<ExtraHitWrapper> TriggerOtherHittables(Transform obj, GameObject attacker, int damage, AttackTypes type, float hitDirectionDegrees)
        {
            return TriggerOtherHittables(obj, new HitInfo {
                Attacker = attacker,
                Damage = damage,
                AttackStrength = 1f,
                AttackType = type,
                Direction = hitDirectionDegrees,
                IgnoreInvincible = false
            });
        }

        public static List<ExtraHitWrapper> TriggerOtherHittables(Transform obj, HitInfo hit)
        {
            List<ExtraHitWrapper> hitObjects = new List<ExtraHitWrapper>();

            int depth = 0;

			while (obj != null)
			{
                foreach (var hittable in GetOtherHittables(obj))
                {
                    var hitInfo = new HitInfo {
                        Attacker = hit.Attacker,
                        Damage = hit.Damage,
                        AttackStrength = hit.AttackStrength,
                        AttackType = hit.AttackType,
                        Direction = hit.Direction,
                        IgnoreInvincible = hit.IgnoreInvincible
                    };
                    ApplyEnemyDamageModifier(obj.gameObject, ref hitInfo);

                    hittable.Hit(hit);
                    hitObjects.Add(hittable);
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
