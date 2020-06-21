using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;

namespace WeaverCore.Editor.Implementations
{
    public class E_Boss_I : Boss_I
    {
        public class E_Statics : Boss_I.Statics
        {
            public static Action OnBossesDead;
            public static Action OnBossSceneComplete;


            public override void OnBossesDeadAdd(Action action)
            {
                OnBossesDead += action;
            }

            public override void OnBossesDeadRemove(Action action)
            {
                OnBossesDead -= action;
            }

            public override void OnBossSceneCompleteAdd(Action action)
            {
                OnBossSceneComplete += action;
            }

            public override void OnBossSceneCompleteRemove(Action action)
            {
                OnBossSceneComplete -= action;
            }

            public override void EndBossBattle(float delayTime)
            {
                OnBossesDead?.Invoke();
                WeaverRoutine.Start(EndBossDelayed(delayTime));
            }

            IEnumerator<IWeaverAwaiter> EndBossDelayed(float delayTime)
            {
                yield return new Awaiters.WaitForSeconds(delayTime);
                OnBossSceneComplete?.Invoke();
            }
        }
    }
}
