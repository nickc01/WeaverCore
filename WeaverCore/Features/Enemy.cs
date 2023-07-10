using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;
using WeaverCore.Internal;
using WeaverCore.Utilities;

namespace WeaverCore.Features
{
    /// <summary>
    /// The base class for all enemies
    /// </summary>
    [ShowFeature]
	[RequireComponent(typeof(EntityHealth))]
	public class Enemy : MonoBehaviour
	{
		Dictionary<uint, Coroutine> BoundRoutines;
		HashSet<uint> idList;
		uint idCounter = 1;

		[SerializeField]
		[Tooltip("Should all PlayerDamager components be disabled when the enemy dies? This prevents the enemy from damaging the player after death")]
		bool disableDamagersOnDeath = true;

		/// <summary>
		/// The previous move that was run
		/// </summary>
		public IEnemyMove PreviousMove { get; private set; }
		/// <summary>
		/// The current move that is being run
		/// </summary>
		public IEnemyMove CurrentMove { get; private set; }

		bool currentMoveCancelled = false;

		EntityHealth _health;
		/// <summary>
		/// The enemy's <seealso cref="EntityHealth"/>
		/// </summary>
		public EntityHealth HealthComponent
        {
			get
            {
                if (_health == null)
                {
					_health = GetComponent<EntityHealth>();
				}
				return _health;
            }
        }

		/// <summary>
		/// Should all PlayerDamager components be disabled when the enemy dies?
		/// </summary>
		public bool DisableDamagersOnDeath { get => disableDamagersOnDeath; set => disableDamagersOnDeath = value; }

        Enemy_I enemyImpl;
		static Enemy_I.Statics staticImpl = ImplFinder.GetImplementation<Enemy_I.Statics>();

		/// <summary>
		/// Called when the enemy awakes
		/// </summary>
		protected virtual void Awake()
		{
			var enemyImplType = ImplFinder.GetImplementationType<Enemy_I>();
			enemyImpl = (Enemy_I)gameObject.AddComponent(enemyImplType);
			HealthComponent.OnDeathEvent += OnDeath_Internal;
		}

		/// <summary>
		/// Called when the enemy dies
		/// </summary>
		protected virtual void OnDeath()
		{
			
		}

		void OnDeath_Internal(HitInfo finalHit)
        {
			StopAllBoundRoutines();
			var deathEffects = GetComponents<IDeathEffects>();
            foreach (var deathEffect in deathEffects)
            {
				deathEffect.PlayDeathEffects(finalHit);
            }
            if (disableDamagersOnDeath)
            {
				foreach (var damager in GetComponentsInChildren<PlayerDamager>())
				{
					damager.damageDealt = 0;
				}
			}
			HealthComponent.OnDeathEvent -= OnDeath_Internal;
			OnDeath();
			if (CurrentMove != null)
			{
				CurrentMove.OnDeath();
			}
		}

		/// <summary>
		/// Causes an object to emit a roar effect
		/// </summary>
		/// <remarks>Must be used in a coroutine function</remarks>
		/// <param name="source">The object that is roaring</param>
		/// <param name="duration">How long the roar will last</param>
		/// <param name="lockPlayer">Whether the player should be locked in place and be prevented from moving</param>
		/// <returns>Returns an IEnumerator for use in a coroutine function</returns>
		public static IEnumerator Roar(GameObject source, float duration, bool lockPlayer = true)
		{
			return Roar(source, duration, null,lockPlayer);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="duration"></param>
		/// <param name="lockPlayer"></param>
		/// <returns></returns>
		public IEnumerator Roar(float duration, bool lockPlayer = true)
		{
			return Roar(gameObject, duration, null, lockPlayer);
		}

		/// <summary>
		/// Causes the enemy to emit a roar effect
		/// </summary>
		/// <remarks>Must be used in a coroutine function</remarks>
		/// <param name="duration">How long the roar will last</param>
		/// <param name="roarSound">The sound that will be played during the roar</param>
		/// <param name="lockPlayer">Whether the player should be locked in place and be prevented from moving</param>
		/// <returns>Returns an IEnumerator for use in a coroutine function</returns>
		public IEnumerator Roar(float duration, AudioClip roarSound, bool lockPlayer = true)
		{
			return Roar(gameObject, duration, roarSound, lockPlayer);
		}

		/// <summary>
		/// Causes an object to emit a roar effect
		/// </summary>
		/// <remarks>Must be used in a coroutine function</remarks>
		/// <param name="source">The object that is roaring</param>
		/// <param name="duration">How long the roar will last</param>
		/// <param name="roarSound">The sound that will be played during the roar</param>
		/// <param name="lockPlayer">Whether the player should be locked in place and be prevented from moving</param>
		/// <returns>Returns an IEnumerator for use in a coroutine function</returns>
		public static IEnumerator Roar(GameObject source, float duration, AudioClip roarSound, bool lockPlayer = true)
		{
			return staticImpl.Roar(source, duration, roarSound, lockPlayer);
		}

		/// <summary>
		/// Causes an object to emit a roar effect
		/// </summary>
		/// <remarks>Must be used in a coroutine function</remarks>
		/// <param name="source">The object that is roaring</param>
		/// <param name="spawnPosition">The position the roar should be occuring at</param>
		/// <param name="duration">How long the roar will last</param>
		/// <param name="lockPlayer">Whether the player should be locked in place and be prevented from moving</param>
		/// <returns>Returns an IEnumerator for use in a coroutine function</returns>
		public static IEnumerator Roar(GameObject source, Vector3 spawnPosition, float duration, bool lockPlayer = true)
		{
			return Roar(source, spawnPosition, duration, null, lockPlayer);
		}

		/// <summary>
		/// Causes the enemy to emit a roar effect
		/// </summary>
		/// <remarks>Must be used in a coroutine function</remarks>
		/// <param name="duration">How long the roar will last</param>
		/// <param name="spawnPosition">The position the roar should be occuring at</param>
		/// <param name="lockPlayer">Whether the player should be locked in place and be prevented from moving</param>
		/// <returns>Returns an IEnumerator for use in a coroutine function</returns>
		public IEnumerator Roar(float duration, Vector3 spawnPosition, bool lockPlayer = true)
		{
			return Roar(gameObject, spawnPosition, duration, null, lockPlayer);
		}

		/// <summary>
		/// Causes the enemy to emit a roar effect
		/// </summary>
		/// <remarks>Must be used in a coroutine function</remarks>
		/// <param name="duration">How long the roar will last</param>
		/// <param name="spawnPosition">The position the roar should be occuring at</param>
		/// <param name="roarSound">The sound that will be played during the roar</param>
		/// <param name="lockPlayer">Whether the player should be locked in place and be prevented from moving</param>
		/// <returns>Returns an IEnumerator for use in a coroutine function</returns>
		public IEnumerator Roar(float duration, Vector3 spawnPosition, AudioClip roarSound, bool lockPlayer = true)
		{
			return Roar(gameObject, spawnPosition, duration, roarSound, lockPlayer);
		}

		/// <summary>
		/// Causes an object to emit a roar effect
		/// </summary>
		/// <remarks>Must be used in a coroutine function</remarks>
		/// <param name="source">The object that is roaring</param>
		/// <param name="spawnPosition">The position the roar should be occuring at</param>
		/// <param name="duration">How long the roar will last</param>
		/// <param name="roarSound">The sound that will be played during the roar</param>
		/// <param name="lockPlayer">Whether the player should be locked in place and be prevented from moving</param>
		/// <returns>Returns an IEnumerator for use in a coroutine function</returns>
		public static IEnumerator Roar(GameObject source, Vector3 spawnPosition, float duration, AudioClip roarSound, bool lockPlayer = true)
		{
			return staticImpl.Roar(source, spawnPosition, duration, roarSound, lockPlayer);
		}


		/// <summary>
		/// Starts a bound coroutine. A bound coroutine is a coroutine that will automatically stop when the boss either dies or gets stunned
		/// </summary>
		/// <returns>Returns an id for stopping it</returns>
		public uint StartBoundRoutine(IEnumerator routine)
		{
			if (BoundRoutines == null)
			{
				BoundRoutines = new Dictionary<uint, Coroutine>();
				idList = new HashSet<uint>();
			}
			uint currentID = idCounter;
			if (idCounter == uint.MaxValue)
			{
				idCounter = 1;
			}
			else
			{
				idCounter++;
			}
			idList.Add(currentID);
			var coroutine = StartCoroutine(DoBoundRoutine(currentID, routine));
			BoundRoutines.Add(currentID, coroutine);
			return currentID;
		}

		/// <summary>
		/// Runs a new move
		/// </summary>
		/// <remarks>Must be used in a coroutine function</remarks>
		/// <param name="move">The move to run</param>
		/// <returns>Returns an IEnumerator for use in a coroutine function</returns>
		public IEnumerator RunMove(IEnemyMove move)
		{
			return RunMoveUntil(move, () => currentMoveCancelled || move != CurrentMove);
		}

		/// <summary>
		/// Runs a new move until the predicate returns false or when the move completes
		/// </summary>
		/// <remarks>Must be used in a coroutine function</remarks>
		/// <param name="move">The move to run</param>
		/// <param name="predicate">The predicate used to stop the move when it returns false</param>
		/// <returns>Returns an IEnumerator for use in a coroutine function</returns>
		public IEnumerator RunMoveUntil(IEnemyMove move, Func<bool> predicate)
		{
			CurrentMove = move;
			currentMoveCancelled = false;
			yield return CoroutineUtilities.RunWhile(move.DoMove(), () => !predicate() && !(currentMoveCancelled || move != CurrentMove));
			PreviousMove = move;
			if (CurrentMove == move)
			{
				CurrentMove = null;
			}
			else if (CurrentMove != null && !currentMoveCancelled)
			{
				move.OnCancel();
			}
		}

		/// <summary>
		/// Stops the <see cref="CurrentMove"/>
		/// </summary>
		public void CancelCurrentMove()
		{
			if (CurrentMove != null)
			{
				CurrentMove.OnCancel();
				currentMoveCancelled = true;
			}
		}

		/// <summary>
		/// Stops the <see cref="CurrentMove"/> without calling <see cref="IEnemyMove.OnCancel"/>
		/// </summary>
		protected void StopCurrentMove()
		{
			if (CurrentMove != null)
			{
				currentMoveCancelled = true;
			}
		}

		/// <summary>
		/// Checks if a bound routine is still running
		/// </summary>
		/// <param name="routineID">The id of the bound routine</param>
		/// <returns>Returns whether the routine is running</returns>
		public bool IsBoundRoutineRunning(uint routineID)
		{
			return idList.Contains(routineID);
		}

		/// <summary>
		/// Stops a bound routine
		/// </summary>
		/// <param name="routineID">The id of the bound routine</param>
		public void StopBoundRoutine(uint routineID)
		{
			if (BoundRoutines == null)
			{
				return;
			}
			if (BoundRoutines.ContainsKey(routineID))
			{
				idList.Remove(routineID);
				BoundRoutines.Remove(routineID);
			}
		}

		/// <summary>
		/// Stops all bound routines
		/// </summary>
		public void StopAllBoundRoutines()
		{
			if (BoundRoutines == null)
			{
				return;
			}
			BoundRoutines.Clear();
			idList.Clear();
		}

		IEnumerator DoBoundRoutine(uint id, IEnumerator routine)
		{
			yield return CoroutineUtilities.RunWhile(routine, () => idList.Contains(id));
			idList.Remove(id);
			BoundRoutines.Remove(id);
		}

		/// <summary>
		/// Called when the enemy gets parried
		/// </summary>
		/// <param name="collider">The collider that recieved the parry</param>
		/// <param name="hit">The hit info of the parried attack</param>
		public virtual void OnParry(IHittable collider, HitInfo hit)
		{

		}
	}
}
