using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;
using WeaverCore.Assets;
using WeaverCore.Enums;
using System.Collections;

namespace WeaverCore.Components
{
	public enum HitResult
	{
		Invalid,
		Invincible,
		Valid
	}

	public class EntityHealth : MonoBehaviour, IHittable, IExtraDamageable
	{
		public delegate void HealthChangeDelegate(int previousHealth, int newHealth);

		List<HealthMilestone> HealthMilestones = new List<HealthMilestone>();


		new Collider2D collider;


		HealthManager_I impl;

		[SerializeField]
		private int _health = 100;
		private float evasionTimer = 0.0f;

		/// <summary>
		/// How much health the enemy has. This gets decreased each time the player hits this object
		/// </summary>
		public int Health
		{
			get
			{
				return _health;
			}
			set
			{
				if (_health != value)
				{
					var oldHealth = _health;
					_health = value;
					OnHealthUpdate(oldHealth,_health);
					if (OnHealthChangeEvent != null)
					{
						OnHealthChangeEvent(oldHealth, _health);
					}
					CheckMilestones(_health);
					if (_health <= 0)
					{
						_health = 0;
						OnDeath();
					}
				}
			}
		}

		/// <summary>
		/// Whether the health should decrease on each hit or not
		/// </summary>
		public bool DecreaseHealth = true;

		/// <summary>
		/// Whether the enemy is invincible to attacks
		/// </summary>
		public bool Invincible = false;

		/// <summary>
		/// If true, it causes all attacks from the player to result in a massive click and deflection. This is only used if invicible is set to true
		/// </summary>
		public bool DeflectBlows = false;


		/// <summary>
		/// Controls how often the enemy is able to receive attacks. 
		/// For example, if the value is set to 0.15, then that means this object will not receive any more hits, until 0.15 seconds have elapsed since the last hit
		/// </summary>
		public float EvasionTime = 0.2f;


		/// <summary>
		/// If true, will cause the player to gain soul points when hit
		/// </summary>
		public bool GainSoul = true;

		/// <summary>
		/// How much evasion time is left <seealso cref="EvasionTime"/>
		/// </summary>
		public float EvasionTimeLeft
		{
			get
			{
				return evasionTimer;
			}
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
		public event Action OnDeathEvent;

		/// <summary>
		/// Called when the entity's health has been modified
		/// </summary>
		public event HealthChangeDelegate OnHealthChangeEvent;

		/// <summary>
		/// Applies an offset to hit effects if desired
		/// </summary>
		public Vector3 EffectsOffset = new Vector3(0, 0,0);

		[SerializeField]
		[Tooltip("If set to true, this enemy will recieve damage from extra abilities, such as spore damage")]
		private bool receiveExtraDamage = true;

		// Token: 0x0400001F RID: 31
		private AudioClip extraDamageClip;

		Recoiler recoil;

		[Space]
		[Space]
		[Header("Geo Dropped on Death")]
		public int SmallGeo = 0;
		public int MediumGeo = 0;
		public int LargeGeo = 0;

		/// <summary>
		/// Hits the target. Returns true if the hit was valid. Meaning, if the hit was able to damage the enemy
		/// </summary>
		/// <param name="hit"></param>
		/// <returns></returns>
		public virtual bool Hit(HitInfo hit)
		{
			LastAttackInfo = hit;
			LastAttackDirection = DirectionUtilities.DegreesToDirection(hit.Direction);
			//WeaverLog.Log("HIT TYPE = " + hit.AttackType);
			//Debug.Log("Hit Direction = " + hit.Direction);
			var hitResult = IsValidHit(hit);
			//WeaverLog.Log("Valid Result = " + hitResult);
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

		/// <summary>
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
		}

		public HitResult IsValidHit(HitInfo hit)
		{
			bool validHit = !(Health <= 0 || EvasionTimeLeft > 0.0f || hit.Damage <= 0 || gameObject.activeSelf == false);
			//WeaverLog.Log("Health = " + Health);
			//WeaverLog.Log("EvasionTimeLeft = " + EvasionTimeLeft);
			//WeaverLog.Log("Hit Damage = " + hit.Damage);
			//WeaverLog.Log("GM Active = " + gameObject.activeSelf);
			//WeaverLog.Log("Attack Type_ = " + hit.AttackType);
			//WeaverLog.Log("Invincible = " + Invincible);
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
			if (player == null)
			{
				//player = Player.GetPlayerFromChild(hit.Attacker);
				player = hit.GetAttackingPlayer();
			}

			//WeaverLog.Log("PLAYING HIT EFFECTS FOR : " + hit.AttackType);

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
					//effect offset y : -0.2
					Pooling.Instantiate(WeaverCore.Assets.EffectAssets.FireballHitPrefab, transform.position + new Vector3(0f, -0.2f, -0.0031f), Quaternion.identity);
					break;
				case AttackType.SharpShadow:
					Pooling.Instantiate(WeaverCore.Assets.EffectAssets.SharpShadowImpactPrefab, transform.position + new Vector3(0f, -0.2f, -0.0031f), Quaternion.identity);
					break;
				default:
					break;
			}

			var hitEffects = GetComponent<IHitEffects>();
			if (hitEffects != null && hit.AttackType != AttackType.RuinsWater)
			{
				hitEffects.PlayHitEffect(hit);
			}
		}

		public void PlayInvincibleHitEffects(HitInfo hit)
		{
			var cardinalDirection = DirectionUtilities.DegreesToDirection(hit.Direction);
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
			//TODO
			var blockedHitEffect = Instantiate(EffectAssets.BlockedHitPrefab, v, Quaternion.identity);
			blockedHitEffect.transform.eulerAngles = eulerAngles;

			WeaverAudio.PlayAtPoint(Assets.AudioAssets.DamageEnemy, transform.position, channel: AudioChannel.Sound);

		}

		void InvincibleHit(HitInfo hit)
		{
			impl.OnInvincibleHit(hit);
			var cardinalDirection = DirectionUtilities.DegreesToDirection(hit.Direction);
			if (DeflectBlows)
			{
				if (hit.AttackType == AttackType.Nail)
				{
					if (cardinalDirection == CardinalDirection.Right)
					{
						//Make the player recoil left : TODO
						//HeroController.instance.RecoilLeft();
						Player.Player1.Recoil(CardinalDirection.Left);
					}
					else if (cardinalDirection == CardinalDirection.Left)
					{
						//Make the player recoil right : TODO
						//HeroController.instance.RecoilRight();
						Player.Player1.Recoil(CardinalDirection.Right);
					}
				}
				//Freeze the game for a moment : TODO
				//GameManager.instance.FreezeMoment(1);
				WeaverGameManager.FreezeGameTime(WeaverGameManager.TimeFreezePreset.Preset1);

				//Make the camera shake : TODO
				//GameCameras.instance.cameraShakeFSM.SendEvent("EnemyKillShake");
				CameraShaker.Instance.Shake(ShakeType.EnemyKillShake);

				PlayInvincibleHitEffects(hit);
				
				evasionTimer = EvasionTime;
			}
		}

		void NormalHit(HitInfo hit)
		{
			//var player = Player.GetPlayerFromChild(hit.Attacker);
			var player = hit.GetAttackingPlayer();
			//If acid is ignored
			/*if (hitInstance.AttackType == AttackTypes.Acid && this.ignoreAcid)
			{
				return;
			}*/
			var cardinalDirection = DirectionUtilities.DegreesToDirection(hit.Direction);

			//Apply recoil if used
			if (this.recoil != null)
			{
				this.recoil.RecoilByDirection(cardinalDirection, hit.AttackStrength);
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
						//HeroController.instance.SoulGain();
						player.SoulGain();
					}
				}
			}


			PlayHitEffects(hit, player);

			/*if (player != null)
			{
				player.PlayAttackSlash(gameObject, hit);
			}

			var hitEffects = GetComponent<IHitEffects>();
			if (hitEffects != null)
			{
				hitEffects.PlayHitEffect(hit);
			}*/

			//Updates the health. If the health is at or below zero, this will also trigger the OnDeath() function
			Health -= hit.Damage;

			if (Health > 0)
			{
				evasionTimer = EvasionTime;
			}

		}

		public void AddHealthMilestone(int health, Action action)
		{
			if (health > Health)
			{
				return;
			}
			HealthMilestones.Add(new HealthMilestone(health, action));
		}

		void CheckMilestones(int healthAfter)
		{
			for (int i = HealthMilestones.Count - 1; i >= 0; i--)
			{
				var milestone = HealthMilestones[i];
				if (milestone.HealthNumber >= healthAfter)
				{
					milestone.MilestoneReached();
					HealthMilestones.RemoveAt(i);
				}
			}
		}

		void IExtraDamageable.RecieveExtraDamage(ExtraDamageTypes extraDamageType)
		{
			if (this.receiveExtraDamage)
			{
				if (!base.gameObject.CompareTag("Spell Vulnerable") && this.Invincible)
				{
					return;
				}
				if (this.extraDamageClip == null)
				{
					this.extraDamageClip = WeaverAssets.LoadWeaverAsset<AudioClip>("Extra Damage Audio");
				}
				AudioPlayer weaverAudioPlayer = WeaverAudio.PlayAtPoint(this.extraDamageClip, base.transform.position, 1f, AudioChannel.Sound);
				weaverAudioPlayer.AudioSource.pitch = UnityEngine.Random.Range(0.85f, 1.15f);
				SpriteFlasher component = base.GetComponent<SpriteFlasher>();
				if (component != null)
				{
					if (extraDamageType != ExtraDamageTypes.Spore)
					{
						if (extraDamageType == ExtraDamageTypes.Dung || extraDamageType == ExtraDamageTypes.Dung2)
						{
							component.FlashDung();
						}
					}
					else
					{
						component.FlashSpore();
					}
				}
				int num = 1;
				if (extraDamageType == ExtraDamageTypes.Dung2)
				{
					num = 2;
				}
				this.Health -= num;
			}
		}

		public void Die()
		{
			if (Health > 0)
			{
				Health = 0;
			}
		}

		/// <summary>
		/// Calls the OnDeath Event, but does not change the health. This is if you only want to trigger the death event and nothing else
		/// </summary>
		public void DoDeathEvent()
		{
			OnDeath();
		}

		protected virtual void OnDeath()
		{
			if (OnDeathEvent != null)
			{
				OnDeathEvent();
			}
			impl.OnDeath();
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
			StartCoroutine(CheckPersistence());
		}

		IEnumerator CheckPersistence()
		{
			yield return null;
			if (impl.ShouldBeDead())
			{
				gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// Called when the health is being updated with a new value
		/// </summary>
		/// <param name="newValue">The new health value</param>
		protected virtual void OnHealthUpdate(int oldHealth, int newHealth)
		{

		}
	}
}
