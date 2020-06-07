using System;
using UnityEngine;
using WeaverCore.Features;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class Enemy_I : MonoBehaviour, IImplementation
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
