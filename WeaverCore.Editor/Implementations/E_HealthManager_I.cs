using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Components;

namespace WeaverCore.Editor.Implementations
{
	class E_HealthManager_I : WeaverCore.Implementations.HealthManager_I
    {

        public override void OnInvincibleHit(HitInfo info)
        {
            
        }

        public override void OnSuccessfulHit(HitInfo info)
        {
            
        }

        public override void OnDeath(HitInfo finalHit)
        {
            
        }

        public override void OnHit(HitInfo info, EntityHealth.HitResult hitResult)
        {
            
        }

		public override bool ShouldBeDead()
		{
            return false;
		}
	}
}
