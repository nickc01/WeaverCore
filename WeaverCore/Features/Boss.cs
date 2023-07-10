using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions.Must;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.Enums;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;


namespace WeaverCore.Features
{
    /// <summary>
    /// The base class for all bosses
    /// </summary>
    [ShowFeature]
	[RequireComponent(typeof(EntityHealth))]
	public class Boss : Enemy
	{
		/// <summary>
		/// The currently set difficulty of the boss scene
		/// </summary>
		public static BossDifficulty Difficulty
		{
			get
			{
				return staticImpl.Difficulty;
			}
			set
			{
				staticImpl.Difficulty = value;
			}
		}

		/// <summary>
		/// Whether the boss is currently in a pantheon
		/// </summary>
		public static bool InPantheon
		{
			get
			{
				return staticImpl.InPantheon;
			}
		}

		/// <summary>
		/// Whether the boss is in a god home arena or not
		/// </summary>
		public static bool InGodHomeArena
		{
			get
			{
				return staticImpl.InGodHomeArena;
			}
		}

		[SerializeField]
		int _bossStage = 1;
		/// <summary>
		/// The current stage/phase the boss is at. The boss begins on boss stage 1 and is incremented every time the boss is stunned
		/// </summary>
		public int BossStage
		{
			get
			{
				return _bossStage;
			}
			set
			{
				_bossStage = value;
			}
		}

		Boss_I impl;
		static Boss_I.Statics staticImpl = ImplFinder.GetImplementation<Boss_I.Statics>();

		/// <summary>
		/// Called when the boss awakes
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			var bossImplType = ImplFinder.GetImplementationType<Boss_I>();
			impl = (Boss_I)gameObject.AddComponent(bossImplType);
		}

		/// <summary>
		/// Triggers the ending transition to play
		/// </summary>
		/// <param name="delay">The delay before the transition is played</param>
		public static void EndBossBattle(float delay = 0f)
		{
			staticImpl.EndBossBattle(delay);
		}

		/// <summary>
		/// Adds a health milestone that will stun the boss when it is reached.
		/// </summary>
		/// <param name="health">The health milestone. When the boss's health goes below this value, the stun will trigger</param>
		public void AddStunMilestone(int health)
		{
			HealthComponent.AddHealthMilestone(health, Stun);
		}

		/// <summary>
		/// Stuns the boss and moves it to the next stage
		/// </summary>
		public void Stun()
		{
			if (CurrentMove != null && CurrentMove is IBossMove bMove)
			{
				bMove.OnStun();
			}
			StopCurrentMove();
			OnStun();
		}

		/// <summary>
		/// Called when the boss is stunned
		/// </summary>
		protected virtual void OnStun()
		{
			StopAllBoundRoutines();
			BossStage++;
		}

		/// <summary>
		/// Called when the boss dies
		/// </summary>
		protected override void OnDeath()
		{
			BossStage++;
		}
	}
}
