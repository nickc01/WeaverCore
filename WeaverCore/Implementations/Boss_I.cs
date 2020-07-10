using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class Boss_I : MonoBehaviour, IImplementation
	{
		public abstract class Statics : IImplementation
		{
			public abstract void OnBossesDeadAdd(Action action);
			public abstract void OnBossesDeadRemove(Action action);
			public abstract void OnBossSceneCompleteAdd(Action action);
			public abstract void OnBossSceneCompleteRemove(Action action);

			public abstract void EndBossBattle(float delayTime);
		}
	}
}
