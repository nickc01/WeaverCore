using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions.Must;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.DataTypes;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;




namespace WeaverCore.Features
{
	[ShowFeature]
	[RequireComponent(typeof(EntityHealth))]
	public class Boss : Enemy
	{
		public static BossDifficulty Diffculty
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

		[SerializeField]
		int _bossStage = 1;
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
		List<IBossMove> bossMoves = new List<IBossMove>();
		
		public IBossMove PreviousMove { get; private set; }
		public IBossMove CurrentMove { get; private set; }

		EntityHealth entityHealth;

		Boss_I impl;
		static Boss_I.Statics staticImpl = ImplFinder.GetImplementation<Boss_I.Statics>();

		protected virtual void Awake()
		{
			//BossStage = 1;
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
			var allMovesCache = new List<IBossMove>(AllMoves);
			var randomList = new List<IBossMove>(AllMoves);
			IBossMove previousMove = null;
			while (true)
			{
				if (allMovesCache.GetElementsHash() != AllMoves.GetElementsHash())
				{
					randomList = new List<IBossMove>(AllMoves);
					allMovesCache = new List<IBossMove>(AllMoves);
				}
				/*if (!randomList.AreEquivalent(AllMoves))
				{
					randomList
				}*/
				randomList.Sort(Randomizer<IBossMove>.Instance);

				if (previousMove != null && randomList.Count > 0 && randomList[0] == previousMove)
				{
					randomList.RemoveAt(0);
					randomList.Add(previousMove);
					previousMove = null;
				}

				bool returnedOnce = false;
				foreach (var move in randomList)
				{
					if (move.MoveEnabled)
					{
						returnedOnce = true;
						previousMove = move;
						yield return move;
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
			return RunMoveUntil(move, () => stage != BossStage || move != CurrentMove);
		}

		public IEnumerator RunMoveUntil(IBossMove move, Func<bool> predicate)
		{
			CurrentMove = move;
			yield return CoroutineUtilities.RunWhile(move.DoMove(), () => !predicate());
			PreviousMove = move;
			if (CurrentMove == move)
			{
				CurrentMove = null;
			}
			else
			{
				move.OnCancel();
			}
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
