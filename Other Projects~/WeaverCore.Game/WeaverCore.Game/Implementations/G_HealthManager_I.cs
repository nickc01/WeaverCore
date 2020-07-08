using IL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.DataTypes;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Implementations
{
	public class G_HealthManager_I : WeaverCore.Implementations.HealthManager_I
    {
        public override void OnDeath()
        {
            
        }

        public override void OnHit(HitInfo info, HitResult hitResult)
        {
            if (hitResult != HitResult.Invalid)
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
