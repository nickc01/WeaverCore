using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;
using WeaverCore.Helpers;

namespace WeaverCore.Features
{
	public class Enemy : Feature
	{
		public static event Action OnBossesDead
		{
			add => staticImpl.OnBossesDeadAdd(value);
			remove => staticImpl.OnBossesDeadRemove(value);
		}
		public static event Action OnBossSceneComplete
		{
			add => staticImpl.OnBossSceneCompleteAdd(value);
			remove => staticImpl.OnBossSceneCompleteRemove(value);
		}

		EnemyImplementation enemyImpl;
		static EnemyStaticImplementation staticImpl = ImplFinder.GetImplementation<EnemyStaticImplementation>();

		protected virtual void Start()
		{
			var enemyImplType = ImplFinder.GetImplementationType<EnemyImplementation>();
			enemyImpl = (EnemyImplementation)gameObject.AddComponent(enemyImplType);
		}

		public static void EndBossBattle(float delayTime)
		{
			staticImpl.EndBossBattle(delayTime);
		}
	}
}
