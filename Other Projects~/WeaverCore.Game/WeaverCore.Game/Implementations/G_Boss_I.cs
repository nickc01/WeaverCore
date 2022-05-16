using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Enums;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{

    public class G_Boss_I : Boss_I
    {
        public class G_Statics : Boss_I.Statics
        {
            public override BossDifficulty Difficulty
            {
                get
                {
                    if (BossSceneController.Instance != null)
                    {
                        return (BossDifficulty)BossSceneController.Instance.BossLevel;
                    }
                    return BossDifficulty.Attuned;
                }
                set
                {
                    if (BossSceneController.Instance != null)
                    {
                        BossSceneController.Instance.BossLevel = (int)value;
                    }
                }
            }

            public override bool InPantheon
            {
                get
                {
                    return BossSceneController.Instance != null;
                }
            }

            public override bool InGodHomeArena => BossSceneController.IsBossScene;

            public override void OnBossesDeadAdd(Action action)
            {
                if (BossSceneController.Instance != null)
                {
                    BossSceneController.Instance.OnBossesDead += action;
                }
            }

            public override void OnBossesDeadRemove(Action action)
            {
                if (BossSceneController.Instance != null)
                {
                    BossSceneController.Instance.OnBossesDead -= action;
                }
            }

            public override void OnBossSceneCompleteAdd(Action action)
            {
                if (BossSceneController.Instance != null)
                {
                    BossSceneController.Instance.OnBossSceneComplete += action;
                }
            }

            public override void OnBossSceneCompleteRemove(Action action)
            {
                if (BossSceneController.Instance != null)
                {
                    BossSceneController.Instance.OnBossSceneComplete -= action;
                }
            }

            public override void EndBossBattle(float delayTime)
            {
                if (BossSceneController.Instance != null)
                {
                    BossSceneController.Instance.bossesDeadWaitTime = delayTime;
                    BossSceneController.Instance.EndBossScene();
                }
            }
        }
    }
}
