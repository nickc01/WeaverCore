using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Helpers;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;
using WeaverCore.WeaverAssets;

namespace WeaverCore.Components
{
	public class HealthManager : MonoBehaviour, IHittable
	{
		Collider2D collider;


		HealthManagerImplementation impl;

		[SerializeField]
		private int health = 100;
		private float evasionTimer = 0.0f;

		/// <summary>
		/// How much health the enemy has. This gets decreased each time the player hits this object
		/// </summary>
		public int Health
		{
			get => health;
			set
			{
				health = value;
				if (health <= 0)
				{
					health = 0;
					OnDeath();
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
		/// Applies an offset to hit effects if desired
		/// </summary>
		public Vector3 EffectsOffset = new Vector3(0, 0,0);


		[Space]
		[Space]
		[Header("Geo Dropped on Death")]
		public int SmallGeo = 0;
		public int MediumGeo = 0;
		public int LargeGeo = 0;

		public virtual void Hit(HitInfo hit)
		{
			bool validHit = !(health <= 0 || EvasionTimeLeft > 0.0f || hit.Damage <= 0 || gameObject.activeSelf == false);
			if (!validHit)
			{
				return;
			}
			impl.OnHit(hit,validHit);

			if (!Invincible || ((hit.AttackType == AttackType.Spell || hit.AttackType == AttackType.SharpShadow) && gameObject.CompareTag("Spell Vulnerable")))
			{
				NormalHit(hit);
			}
			else
			{
				InvincibleHit(hit);
			}
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

				HollowPlayer.Play(WeaverAssets.AudioAssets.DamageEnemy, transform.position, channel: AudioChannel.Sound);

				evasionTimer = EvasionTime;
			}
		}

		void NormalHit(HitInfo hit)
		{
			var player = Player.GetPlayerFromChild(hit.Attacker);
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

			if (hit.AttackType == AttackType.Nail || hit.AttackType == AttackType.NailBeam)
			{
				if (GainSoul)
				{
					//Cause the player to gain soul
					//HeroController.instance.SoulGain();
				}
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

			//Updates the health. If the health is at or below zero, this will also trigger the OnDeath() function
			Health -= hit.Damage;

			if (health > 0)
			{
				evasionTimer = EvasionTime;
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
			impl.OnDeath();
		}

		void Update()
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

		void Awake()
		{
			impl = (HealthManagerImplementation)gameObject.AddComponent(ImplFinder.GetImplementationType<HealthManagerImplementation>());
			impl.Manager = this;
		}
	}
}
