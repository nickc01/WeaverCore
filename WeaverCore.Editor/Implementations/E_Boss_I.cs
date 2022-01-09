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
using WeaverCore.Enums;

namespace WeaverCore.Editor.Implementations
{
	public class E_Boss_I : Boss_I
    {
        public class E_Statics : Boss_I.Statics
        {
            public static Action OnBossesDead;
            public static Action OnBossSceneComplete;

            static BossDifficulty difficulty;
            public override BossDifficulty Difficulty
            {
                get
                {
                    return difficulty;
                }

                set
                {
                    difficulty = value;
                }
            }

            public override bool InPantheon
            {
                get
                {
                    return false;
                }
            }

            public override bool InGodHomeArena
            {
                get
                {
                    return false;
                }
            }

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
                if (OnBossesDead != null)
                {
                    OnBossesDead.Invoke();
                }
                UnboundCoroutine.Start(EndBossDelayed(delayTime));
            }

            IEnumerator EndBossDelayed(float delayTime)
            {
                yield return new WaitForSeconds(delayTime);
                if (OnBossSceneComplete != null)
                {
                    OnBossSceneComplete.Invoke();
                }
            }
        }
    }
}
