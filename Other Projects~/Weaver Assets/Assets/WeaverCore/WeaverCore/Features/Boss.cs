using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions.Must;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Features
{
	[ShowFeature]
	[RequireComponent(typeof(EntityHealth))]
	public class Boss : Enemy
	{
		public int BossStage { get; set; }
		List<IBossMove> bossMoves = new List<IBossMove>();
		
		public IBossMove PreviousMove { get; private set; }
		public IBossMove CurrentMove { get; private set; }

		EntityHealth entityHealth;

		Boss_I impl;
		static Boss_I.Statics staticImpl = ImplFinder.GetImplementation<Boss_I.Statics>();

		protected virtual void Awake()
		{
			BossStage = 1;
			var bossImplType = ImplFinder.GetImplementationType<Boss_I>();
			impl = (Boss_I)gameObject.AddComponent(bossImplType);
			entityHealth = GetComponent<EntityHealth>();
			entityHealth.OnDeathEvent += OnDeath;
		}

		public IEnumerable<IBossMove> EnabledMoves
		{
			get
			{
				return bossMoves.Where(move => move.MoveEnabled);
			}
		}

		public IEnumerable<IBossMove> AllMoves
		{
			get
			{
				return bossMoves;
			}
		}

		public bool AddMove(IBossMove move)
		{
			if (bossMoves.Contains(move))
			{
				return false;
			}
			else
			{
				bossMoves.Add(move);
				return true;
			}
		}

		public void AddMoves(IEnumerable<IBossMove> moves)
		{
			foreach (var move in moves)
			{
				AddMove(move);
			}
		}

		public bool RemoveMove(IBossMove move)
		{
			if (bossMoves.Contains(move))
			{
				bossMoves.Remove(move);
				return true;
			}
			else
			{
				return false;
			}
		}

		public IBossMove GetRandomMove()
		{
			var enabledMoves = new List<IBossMove>(EnabledMoves);
			if (enabledMoves.Count == 0)
			{
				return null;
			}
			return enabledMoves[UnityEngine.Random.Range(0, enabledMoves.Count)];
		}

		public IEnumerable<IBossMove> RandomMoveIter()
		{
			while (true)
			{
				var randomList = new List<IBossMove>(AllMoves);
				randomList.Sort(Randomizer<IBossMove>.Instance);

				bool returnedOnce = false;
				foreach (var element in randomList)
				{
					if (element.MoveEnabled)
					{
						returnedOnce = true;
						yield return element;
					}
				}
				if (!returnedOnce)
				{
					yield return null;
				}
			}
		}

		public IEnumerator RunRandomMove()
		{
			return RunMove(GetRandomMove());
		}

		public IEnumerator RunMove(IBossMove move)
		{
			int stage = BossStage;
			CurrentMove = move;
			yield return CoroutineUtilities.RunWhile(move.DoMove(), () => stage == BossStage && move == CurrentMove);
			PreviousMove = CurrentMove;
			CurrentMove = null;
		}

		public void AddStunMilestone(int health)
		{
			entityHealth.AddHealthMilestone(health, OnStun);
		}

		public void Stun()
		{
			if (CurrentMove != null)
			{
				CurrentMove.OnStun();
			}
			OnStun();
		}

		protected virtual void OnStun()
		{
			BossStage++;
		}

		protected virtual void OnDeath()
		{
			BossStage++;
			entityHealth.OnDeathEvent -= OnDeath;
		}

	}
}
