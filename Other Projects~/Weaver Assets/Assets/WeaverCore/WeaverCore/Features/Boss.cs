using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore.Features
{
	[ShowFeature]
	public class Boss : Enemy
	{
		public static event Action OnBossesDead
		{
			add
			{
				staticImpl.OnBossesDeadAdd(value);
			}
			remove
			{
				staticImpl.OnBossesDeadRemove(value);
			}
		}
		public static event Action OnBossSceneComplete
		{
			add
			{
				staticImpl.OnBossSceneCompleteAdd(value);
			}
			remove
			{
				staticImpl.OnBossSceneCompleteRemove(value);
			}
		}

		Boss_I impl;
		static Boss_I.Statics staticImpl = ImplFinder.GetImplementation<Boss_I.Statics>();

		void Awake()
		{
			var bossImplType = ImplFinder.GetImplementationType<Boss_I>();
			impl = (Boss_I)gameObject.AddComponent(bossImplType);
		}

		public static void EndBossBattle(float delayTime)
		{
			staticImpl.EndBossBattle(delayTime);
		}
	}
}
