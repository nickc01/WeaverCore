using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Utilities;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class HealthManager_I : MonoBehaviour, IImplementation
	{
		[HideInInspector]
		public WeaverCore.Components.EntityHealth Manager;

		public abstract void OnHit(HitInfo info, EntityHealth.HitResult hitResult);
		public abstract void OnInvincibleHit(HitInfo info);
		public abstract void OnSuccessfulHit(HitInfo info);

		public abstract bool ShouldBeDead();

		public abstract void OnDeath(HitInfo finalHit);
	}
}
