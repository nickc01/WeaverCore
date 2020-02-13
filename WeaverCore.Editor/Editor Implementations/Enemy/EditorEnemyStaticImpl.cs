using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore;
using WeaverCore.Implementations;
using WeaverCore.Helpers;

namespace WeaverCore.Editor.Implementations
{
	public class EditorEnemyStaticImplementation : EnemyStaticImplementation
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
            CoroutineStarter.StartCoroutine(EndBossDelayed(delayTime));
        }

        IEnumerator EndBossDelayed(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            OnBossSceneComplete?.Invoke();
        }
    }
}
