using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore.Assets;
using WeaverCore.Enums;
using WeaverCore.Features;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;
using WeaverCore.Internal;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// Used to keep track of the health of an enemy
    /// </summary>
    public class EntityHealth : MonoBehaviour, IHittable, IExtraDamageable, IOnPool
    {
        /// <summary>
        /// The enum used when determining if a hit on the enemy was valid
        /// </summary>
        public enum HitResult
        {
            /// <summary>
            /// The hit dealt some damage to the enemy
            /// </summary>
            Valid,
            /// <summary>
            /// The hit did not deal damage to the enemy
            /// </summary>
            Invalid,
            /// <summary>
            /// The enemy is invincible
            /// </summary>
            Invincible
        }

        //A list of modifiers that are applied when the health is changed
        private SortedSet<IHealthModifier> modifiers = new SortedSet<IHealthModifier>(new IHealthModifier.Sorter());

        public delegate void HealthChangeDelegate(int previousHealth, int newHealth);

        //A list of milestones that get executed when the health reaches certain points
        private List<(int destHealth, Action action)> HealthMilestones = new List<(int destHealth, Action action)>();
        private new Collider2D collider;
        private HealthManager_I impl;

        [SerializeField]
        [Tooltip("The current health value of the enemy")]
        private int _health = 100;
        private float evasionTimer = 0.0f;

        /// <summary>
        /// How much health the enemy has. This gets decreased each time the player hits this enemy
        /// </summary>
        public int Health
        {
            get => _health;
            set
            {
                if (_health != value)
                {
                    int oldHealth = _health;
                    _health = value;

                    foreach (IHealthModifier modifier in modifiers)
                    {
                        _health = modifier.OnHealthChange(oldHealth, _health);
                    }

                    if (OnHealthChangeEvent != null)
                    {
                        OnHealthChangeEvent(oldHealth, _health);
                    }
                    CheckMilestones(_health);
                    if (_health <= 0 && !HasModifier<InfiniteHealthModifier>())
                    {
                        _health = 0;
                        OnDeath(LastAttackInfo);
                    }
                }
            }
        }

        /// <summary>
        /// Whether the enemy is invincible to attacks
        /// </summary>
        public bool Invincible = false;

        /// <summary>
        /// If true, it causes all attacks from the player to result in a massive clink and deflection. This is only used if invicible is set to true
        /// </summary>
        public bool DeflectBlows = false;


        /// <summary>
        /// Controls how often the enemy is able to receive attacks. 
        /// For example, if the value is set to 0.15, then that means this enemy will not receive any more hits, until 0.15 seconds have elapsed since the last hit
        /// </summary>
        public float EvasionTime = 0.2f;


        /// <summary>
        /// If true, will cause the player to gain soul points when hit
        /// </summary>
        public bool GainSoul = true;

        /// <summary>
        /// If true, will play the enemy's hit effects when hit
        /// </summary>
        public bool DoEffectsOnHit = true;

        /// <summary>
        /// How much evasion time is left <seealso cref="EvasionTime"/>
        /// </summary>
        public float EvasionTimeLeft
        {
            get => evasionTimer;
            set
            {
                if (value < 0.0f)
                {
                    value = 0.0f;
                }
                evasionTimer = value;
            }
        }

        /// <summary>
        /// The direction the last attack came from
        /// </summary>
        public CardinalDirection LastAttackDirection { get; private set; }

        /// <summary>
        /// Contains info about the most recent hit on the enemy
        /// </summary>
        public HitInfo LastAttackInfo { get; private set; }

        /// <summary>
        /// Called when the entity's health reaches zero
        /// </summary>
        public event Action<HitInfo> OnDeathEvent;

        /// <summary>
        /// Called when the entity's health has been modified
        /// </summary>
        public event HealthChangeDelegate OnHealthChangeEvent;

        /// <summary>
        /// Applies an offset to hit effects if desired
        /// </summary>
        public Vector3 EffectsOffset = new Vector3(0, 0, 0);

        [SerializeField]
        [Tooltip("If set to true, this enemy will receive damage from extra abilities, such as spore damage")]
        private bool receiveExtraDamage = true;

        private AudioClip extraDamageClip;
        private Recoiler recoil;

        [Space]
        [Space]
        [Header("Geo Dropped on Death")]
        public int SmallGeo = 0;
        public int MediumGeo = 0;
        public int LargeGeo = 0;

        /// <summary>
        /// Hits the target. Returns true if the hit was valid and damage to the enemy was dealt
        /// </summary>
        /// <param name="hit">The hit on the enemy</param>
        /// <returns>Returns whether the hit was a valid hit or not</returns>
        public virtual bool Hit(HitInfo hit)
        {
            LastAttackInfo = hit;
            LastAttackDirection = DirectionUtilities.DegreesToDirection(hit.Direction);
            HitResult hitResult = IsValidHit(hit);
            impl.OnHit(hit, hitResult);
            switch (hitResult)
            {
                case HitResult.Invalid:
                    return false;
                case HitResult.Invincible:
                    InvincibleHit(hit);
                    return false;
                case HitResult.Valid:
                    NormalHit(hit);
                    return true;
                default:
                    return false;
            }
        }

        /*/// <summary>
		/// Sets the health without triggering the <see cref="OnHealthUpdate(int)"/> function or triggering <see cref="OnHealthChangeEvent"/>.
		/// 
		/// Should be used sparringly
		/// </summary>
		/// <param name="newHealth">The new health value</param>
		/// <param name="triggerMilestones">Whether milestones should also be triggered</param>
		protected void SetHealthInternal(int newHealth, bool triggerMilestones = false)
		{
			if (newHealth != _health)
			{
				_health = newHealth;
				if (triggerMilestones)
				{
					CheckMilestones(newHealth);
				}
			}
		}*/

        /// <summary>
        /// Sets the new health without triggering any <see cref="IHealthModifier"/>s, events, or milestones
        /// </summary>
        /// <param name="newHealth">The new health value to set</param>
        public void SetHealthRaw(int newHealth)
        {
            _health = newHealth;
        }

        /// <summary>
        /// Adds a health modifier. The modifier will be used to modify how the health gets changed when the player hits the enemy
        /// </summary>
        /// <param name="modifier">The modifier to be added</param>
        public void AddModifier(IHealthModifier modifier)
        {
            modifiers.Add(modifier);
        }

        /// <summary>
        /// Adds a health modifier. The modifier will be used to modify how the health gets changed when the player hits the enemy
        /// </summary>
        /// <typeparam name="T">The type of health modifier to add</typeparam>
        /// <returns>Returns an instance of the modifier that was added</returns>
        public T AddModifier<T>() where T : IHealthModifier, new()
        {
            T instance = new T();
            AddModifier(instance);
            return instance;
        }

        /// <summary>
        /// Has a health modifier of the specific type been added?
        /// </summary>
        /// <typeparam name="T">The type of health modifier to check for</typeparam>
        /// <returns>Returns whether the modifier has been added</returns>
        public bool HasModifier<T>()
        {
            return modifiers.Any(m => m is T);
        }

        /// <summary>
        /// Has a health modifier of the specific type been added?
        /// </summary>
        /// <typeparam name="T">The type of health modifier to check for</typeparam>
        /// <param name="modifier">If the modifier has been added, this will be a reference to the modifier that is added</param>
        /// <returns>Returns whether the modifier has been added</returns>
        public bool HasModifier<T>(out T modifier) where T : IHealthModifier
        {
            modifier = modifiers.OfType<T>().FirstOrDefault();
            return modifier != null;
        }

        /// <summary>
        /// Removes a health modifier
        /// </summary>
        /// <param name="modifier">The modifier to be removed</param>
        /// <returns>Returns true if the modifier has been successfully removed</returns>
        public bool RemoveModifier(IHealthModifier modifier)
        {
            return modifiers.Remove(modifier);
        }

        /// <summary>
        /// Removes a modifier of the specified type
        /// </summary>
        /// <typeparam name="T">The type of modifier to be removed</typeparam>
        /// <returns>Returns true if the modifier has been successfully removed</returns>
        public bool RemoveModifier<T>() where T : IHealthModifier
        {
            return modifiers.RemoveWhere(m => m is T) > 0;
        }

        /// <summary>
        /// Checks if a hit on this enemy is valid
        /// </summary>
        /// <param name="hit">The hit to check for</param>
        /// <returns>Returns whether this hit is valid</returns>
        public HitResult IsValidHit(HitInfo hit)
        {
            bool validHit = !((Health <= 0 && !HasModifier<InfiniteHealthModifier>()) || EvasionTimeLeft > 0.0f || hit.Damage <= 0 || gameObject.activeSelf == false);
            if (!validHit)
            {
                return HitResult.Invalid;
            }
            if (!Invincible || ((hit.AttackType == AttackType.Spell || hit.AttackType == AttackType.SharpShadow) && gameObject.CompareTag("Spell Vulnerable")))
            {
                return HitResult.Valid;
            }
            else
            {
                return HitResult.Invincible;
            }
        }

        public void PlayHitEffects(HitInfo hit, Player player = null)
        {
            if (!DoEffectsOnHit)
            {
                return;
            }
            if (player == null)
            {
                player = hit.GetAttackingPlayer();
            }

            switch (hit.AttackType)
            {
                case AttackType.NailBeam:
                case AttackType.Generic:
                case AttackType.Nail:
                    if (player != null)
                    {
                        player.PlayAttackSlash(gameObject, hit);
                    }
                    break;
                case AttackType.Spell:
                    Pooling.Instantiate(WeaverCore.Assets.EffectAssets.FireballHitPrefab, transform.position + new Vector3(0f, -0.2f, -0.0031f), Quaternion.identity);
                    break;
                case AttackType.SharpShadow:
                    Pooling.Instantiate(WeaverCore.Assets.EffectAssets.SharpShadowImpactPrefab, transform.position + new Vector3(0f, -0.2f, -0.0031f), Quaternion.identity);
                    break;
                default:
                    break;
            }

            IHitEffects hitEffects = GetComponent<IHitEffects>();
            if (hitEffects != null && hit.AttackType != AttackType.RuinsWater)
            {
                hitEffects.PlayHitEffect(hit);
            }
        }

        public void PlayInvincibleHitEffects(HitInfo hit)
        {
            if (!DoEffectsOnHit)
            {
                return;
            }
            NailDeflect.PlayDeflectEffects(hit.Attacker.transform.position, gameObject, DirectionUtilities.DegreesToDirection(hit.Direction), false);
            //Freeze the game for a moment
            /*WeaverGameManager.FreezeGameTime(WeaverGameManager.TimeFreezePreset.Preset1);

            CameraShaker.Instance.Shake(ShakeType.EnemyKillShake);

            CardinalDirection cardinalDirection = DirectionUtilities.DegreesToDirection(hit.Direction);
            Vector2 v;
            Vector3 eulerAngles;

            if (collider == null)
            {
                collider = GetComponent<Collider2D>();
            }

            if (collider != null)
            {
                switch (cardinalDirection)
                {
                    case CardinalDirection.Up:
                        v = new Vector2(hit.Attacker.transform.position.x, Mathf.Max(hit.Attacker.transform.position.y, collider.bounds.min.y));
                        eulerAngles = new Vector3(0f, 0f, 90f);
                        break;
                    case CardinalDirection.Down:
                        v = new Vector2(hit.Attacker.transform.position.x, Mathf.Min(hit.Attacker.transform.position.y, collider.bounds.max.y));
                        eulerAngles = new Vector3(0f, 0f, 270f);
                        break;
                    case CardinalDirection.Left:
                        v = new Vector2(collider.bounds.max.x, hit.Attacker.transform.position.y);
                        eulerAngles = new Vector3(0f, 0f, 180f);
                        break;
                    case CardinalDirection.Right:
                        v = new Vector2(collider.bounds.min.x, hit.Attacker.transform.position.y);
                        eulerAngles = new Vector3(0f, 0f, 0f);
                        break;
                    default:
                        v = transform.position;
                        eulerAngles = new Vector3(0f, 0f, 0f);
                        break;
                }
            }
            else
            {
                v = transform.position;
                eulerAngles = new Vector3(0f, 0f, 0f);
            }
            GameObject blockedHitEffect = Instantiate(EffectAssets.BlockedHitPrefab, v, Quaternion.identity);
            blockedHitEffect.transform.eulerAngles = eulerAngles;

            WeaverAudio.PlayAtPoint(Assets.AudioAssets.SwordCling, transform.position, channel: AudioChannel.Sound);*/

        }

        private void InvincibleHit(HitInfo hit)
        {
            impl.OnInvincibleHit(hit);
            CardinalDirection cardinalDirection = DirectionUtilities.DegreesToDirection(hit.Direction);
            if (DeflectBlows)
            {
                Debug.Log("ATTACK TYPE = " + hit.AttackType);
                if (hit.AttackType == AttackType.Nail)
                {
                    Debug.Log("DIRECTION = " + cardinalDirection);
                    switch (cardinalDirection)
                    {
                        case CardinalDirection.Up:
                            Player.Player1.Recoil(CardinalDirection.Down);
                            break;
                        case CardinalDirection.Left:
                            Player.Player1.Recoil(CardinalDirection.Right);
                            break;
                        case CardinalDirection.Right:
                            Player.Player1.Recoil(CardinalDirection.Left);
                            break;
                    }
                    /*if (cardinalDirection == CardinalDirection.Right)
                    {
                        //Make the player recoil left
                        Player.Player1.Recoil(CardinalDirection.Left);
                    }
                    else if (cardinalDirection == CardinalDirection.Left)
                    {
                        //Make the player recoil right
                        Player.Player1.Recoil(CardinalDirection.Right);
                    }*/
                }

                PlayInvincibleHitEffects(hit);

                evasionTimer = EvasionTime;
            }
        }

        private void NormalHit(HitInfo hit)
        {
            Player player = hit.GetAttackingPlayer();
            //If acid is ignored
            /*if (hitInstance.AttackType == AttackTypes.Acid && this.ignoreAcid)
			{
				return;
			}*/
            CardinalDirection cardinalDirection = DirectionUtilities.DegreesToDirection(hit.Direction);

            //Apply recoil if used
            if (recoil != null)
            {
                recoil.RecoilByDirection(cardinalDirection, hit.AttackStrength);
            }
            //Play Attack Effects

            impl.OnSuccessfulHit(hit);

            if (player != null)
            {
                if (hit.AttackType == AttackType.Nail || hit.AttackType == AttackType.NailBeam)
                {
                    if (GainSoul)
                    {
                        //Cause the player to gain soul
                        player.SoulGain();
                    }
                }
            }


            PlayHitEffects(hit, player);

            evasionTimer = EvasionTime;

            Health -= hit.Damage;
        }

        public void AddHealthMilestone(int health, Action action)
        {
            if (!HasModifier<InfiniteHealthModifier>() && health > Health)
            {
                action();
                return;
            }
            else if (HasModifier<InfiniteHealthModifier>() && health < Health)
            {
                action();
                return;
            }
            HealthMilestones.Add((health, action));
        }

        private void CheckMilestones(int healthAfter)
        {
            for (int i = HealthMilestones.Count - 1; i >= 0; i--)
            {
                var milestone = HealthMilestones[i];
                if (HasModifier<InfiniteHealthModifier>())
                {
                    if (milestone.destHealth <= healthAfter)
                    {
                        milestone.action();
                        HealthMilestones.RemoveAt(i);
                    }
                }
                else
                {
                    if (milestone.destHealth >= healthAfter)
                    {
                        milestone.action();
                        HealthMilestones.RemoveAt(i);
                    }
                }
            }
        }

        void IExtraDamageable.RecieveExtraDamage(ExtraDamageTypes extraDamageType)
        {
            if (receiveExtraDamage)
            {
                if (!gameObject.CompareTag("Spell Vulnerable") && Invincible)
                {
                    return;
                }
                if (DoEffectsOnHit)
                {
                    if (extraDamageClip == null)
                    {
                        extraDamageClip = WeaverAssets.LoadWeaverAsset<AudioClip>("Extra Damage Audio");
                    }
                    AudioPlayer weaverAudioPlayer = WeaverAudio.PlayAtPoint(extraDamageClip, base.transform.position, 1f, AudioChannel.Sound);
                    weaverAudioPlayer.AudioSource.pitch = UnityEngine.Random.Range(0.85f, 1.15f);

                    foreach (var component in GetComponentsInChildren<SpriteFlasher>())
                    {
                        if (component != null)
                        {
                            if (extraDamageType != ExtraDamageTypes.Spore)
                            {
                                if (extraDamageType == ExtraDamageTypes.Dung || extraDamageType == ExtraDamageTypes.Dung2)
                                {
                                    component.flashDungQuick();
                                }
                            }
                            else
                            {
                                component.flashSporeQuick();
                            }
                        }
                    }
                }
                /*int num = 1;
                if (extraDamageType == ExtraDamageTypes.Dung2)
                {
                    num = 2;
                }*/
                Health -= GetDamageOfType(extraDamageType);
            }
        }

        public static int GetDamageOfType(ExtraDamageTypes extraDamageTypes)
        {
            if ((uint)extraDamageTypes <= 1u || extraDamageTypes != ExtraDamageTypes.Dung2)
            {
                return 1;
            }
            return 2;
        }

        /// <summary>
        /// Forces the entity to die
        /// </summary>
        public void Die()
        {
            Die(LastAttackInfo);
        }

        /// <summary>
        /// Forces the entity to die
        /// </summary>
        /// <param name="finalHit">The final hit on the entity</param>
        public void Die(HitInfo finalHit)
        {
            if (Health > 0)
            {
                Health = 0;
            }
            if (HasModifier<InfiniteHealthModifier>())
            {
                OnDeath(finalHit);
            }
        }

        /// <summary>
        /// Calls the OnDeath Event, but does not change the health. This is if you only want to trigger the death event and nothing else
        /// </summary>
        public void DoDeathEvent()
        {
            DoDeathEvent(LastAttackInfo);
        }

        /// <summary>
        /// Calls the OnDeath Event, but does not change the health. This is if you only want to trigger the death event and nothing else
        /// </summary>
        /// <param name="finalHit">The final hit on the entity</param>
        public void DoDeathEvent(HitInfo finalHit)
        {
            OnDeath(finalHit);
        }

        /// <summary>
        /// Triggered when the entity dies
        /// </summary>
        /// <param name="finalHit">The final hit on the entity</param>
        protected virtual void OnDeath(HitInfo finalHit)
        {
            if (OnDeathEvent != null)
            {
                OnDeathEvent(finalHit);
            }
            impl.OnDeath(finalHit);
        }

        protected virtual void Update()
        {
            if (evasionTimer > 0.0f)
            {
                evasionTimer -= Time.deltaTime;
                if (evasionTimer < 0.0f)
                {
                    evasionTimer = 0.0f;
                }
            }
        }

        protected virtual void Awake()
        {
            impl = (HealthManager_I)gameObject.AddComponent(ImplFinder.GetImplementationType<HealthManager_I>());
            impl.Manager = this;
            recoil = GetComponent<Recoiler>();
            if (Player.Player1 != null)
            {
                Player.Player1.RefreshSoulUI();
            }
            var enemy = GetComponent<Enemy>();
            if (enemy is Boss)
            {
                EnemyHPBars_Interop.MarkAsBoss(gameObject);
            }
            else
            {
                EnemyHPBars_Interop.MarkAsNonBoss(gameObject);
            }
            StartCoroutine(CheckPersistence());
        }

        private IEnumerator CheckPersistence()
        {
            yield return null;
            if (impl.ShouldBeDead())
            {
                gameObject.SetActive(false);
            }
        }

        static Type DisableHPBarType = null;
        static Type BossMarkerType = null;

        void IOnPool.OnPool()
        {
            if (DisableHPBarType == null || BossMarkerType == null)
            {
                foreach (var comp in GetComponents<MonoBehaviour>())
                {
                    var type = comp.GetType();
                    if (type.Name == "DisableHPBar")
                    {
                        DisableHPBarType = type;
                    }
                    else if (type.Name == "BossMarker")
                    {
                        BossMarkerType = type;
                    }
                }
            }

            EnemyHPBars_Interop.DisableHPBar(gameObject);

            if (DisableHPBarType != null && TryGetComponent(DisableHPBarType,out var disableBarComp))
            {
                Destroy(disableBarComp);
            }

            if (BossMarkerType != null && TryGetComponent(BossMarkerType,out var markerComp))
            {
                Destroy(markerComp);
            }
        }
    }
}
