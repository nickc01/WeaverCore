using IL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Helpers;

namespace WeaverCore.Game.Implementations
{
    public class GameHealthManagerImplementation : WeaverCore.Implementations.HealthManagerImplementation
    {
        public override void OnDeath()
        {
            
        }

        public override void OnHit(HitInfo info, bool validHit)
        {
            if (validHit)
            {
                FSMUtility.SendEventToGameObject(info.Attacker, "DEALT DAMAGE", false);
            }
        }

        public override void OnInvincibleHit(HitInfo info)
        {
            FSMUtility.SendEventToGameObject(info.Attacker, "HIT LANDED", false);
        }

        public override void OnSuccessfulHit(HitInfo info)
        {
            FSMUtility.SendEventToGameObject(info.Attacker, "HIT LANDED", false);
        }
    }
}
