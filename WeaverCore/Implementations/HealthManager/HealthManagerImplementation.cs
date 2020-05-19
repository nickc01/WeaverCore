using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Helpers;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class HealthManagerImplementation : MonoBehaviour, IImplementation
	{
		public HealthManager Manager;

		public abstract void OnHit(HitInfo info, bool validHit);
		public abstract void OnInvincibleHit(HitInfo info);
		public abstract void OnSuccessfulHit(HitInfo info);

		public abstract void OnDeath();
	}
}
