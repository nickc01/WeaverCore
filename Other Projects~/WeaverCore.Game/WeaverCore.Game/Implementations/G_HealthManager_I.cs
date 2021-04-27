using IL;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.DataTypes;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Implementations
{
	public class G_HealthManager_I : WeaverCore.Implementations.HealthManager_I
    {
        void Awake()
		{
			if (GetComponent<HealthManagerProxy>() == null)
			{
                gameObject.AddComponent<HealthManagerProxy>();
			}
			if (GetComponent<EnemyDeathEffectsProxy>() == null)
			{
                gameObject.AddComponent<EnemyDeathEffectsProxy>();
			}
		}

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

        static MethodInfo OnEnableEnemyFunc;

		public override bool ShouldBeDead()
		{
			if (OnEnableEnemyFunc == null)
			{
                OnEnableEnemyFunc = typeof(ModHooks).GetMethod("OnEnableEnemy", BindingFlags.Instance | BindingFlags.NonPublic);
			}
            return (bool)OnEnableEnemyFunc.Invoke(ModHooks.Instance, new object[] { Manager.gameObject, false });
		}
	}
}
