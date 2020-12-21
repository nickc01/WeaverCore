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
using WeaverCore.DataTypes;

namespace WeaverCore.Components
{
	public enum HitResult
	{
		Invalid,
		Invincible,
		Valid
	}

	public class EntityHealth : MonoBehaviour, IHittable
	{
		List<Milestone> HealthMilestones = new List<Milestone>();


		Collider2D collider;


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
					_health = OnHealthUpdate(value);
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
		public float EvasionTime = 0.15f;


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
		/// Called when the entity's health reaches zero
		/// </summary>
		public event Action OnDeathEvent;

		/// <summary>
		/// Applies an offset to hit effects if desired
		/// </summary>
		public Vector3 EffectsOffset = new Vector3(0, 0,0);


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
			var hitResult = IsValidHit(hit);
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

		public HitResult IsValidHit(HitInfo hit)
		{
			bool validHit = !(Health <= 0 || EvasionTimeLeft > 0.0f || hit.Damage <= 0 || gameObject.activeSelf == false);
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

			if (player != null)
			{
				player.PlayAttackSlash(gameObject, hit);
			}

			var hitEffects = GetComponent<IHitEffects>();
			if (hitEffects != null)
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
					}
					else if (cardinalDirection == CardinalDirection.Left)
					{
						//Make the player recoil right : TODO
						//HeroController.instance.RecoilRight();
					}
				}
				//Freeze the game for a moment : TODO
				//GameManager.instance.FreezeMoment(1);

				//Make the camera shake : TODO
				//GameCameras.instance.cameraShakeFSM.SendEvent("EnemyKillShake");

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
			/*if (this.recoil != null)
			{
				this.recoil.RecoilByDirection(cardinalDirection, hitInstance.MagnitudeMultiplier);
			}*/
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

				PlayHitEffects(hit, player);
			}

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
			HealthMilestones.Add(new Milestone(health, action));
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

		public void Die()
		{
			if (Health > 0)
			{
				Health = 0;
			}
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
			if (Player.Player1 != null)
			{
				Player.Player1.RefreshSoulUI();
			}
		}

		/// <summary>
		/// Called when the health is being updated with a new value
		/// </summary>
		/// <param name="newValue">The new value that is being set</param>
		/// <returns>Returns the actual value that is going to be set. Use this to override the new value with your own</returns>
		protected virtual int OnHealthUpdate(int newValue)
		{
			return newValue;
		}
	}
}
