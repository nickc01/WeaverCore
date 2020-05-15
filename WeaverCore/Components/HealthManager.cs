using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Helpers;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;

namespace WeaverCore.Components
{
	public class HealthManager : MonoBehaviour, IHitReceiver
	{
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
				if (health == 0)
				{
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

		public virtual void ReceiveHit(HitInfo hit)
		{
			impl.ReceiveHit(hit);
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
