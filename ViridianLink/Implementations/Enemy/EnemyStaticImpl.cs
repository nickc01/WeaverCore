using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViridianLink.Core;

namespace ViridianLink.Implementations
{
    public abstract class EnemyStaticImplementation : IImplementation
    {
        public abstract void OnBossesDeadAdd(Action action);
        public abstract void OnBossesDeadRemove(Action action);
        public abstract void OnBossSceneCompleteAdd(Action action);
        public abstract void OnBossSceneCompleteRemove(Action action);

        public abstract void EndBossBattle(float delayTime);
    }
}
