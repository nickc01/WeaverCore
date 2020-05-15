using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Helpers;

namespace WeaverCore.Implementations
{
	public abstract class HealthManagerImplementation : MonoBehaviour, IImplementation
	{
		public HealthManager Manager;

		public abstract void ReceiveHit(HitInfo info);

		public abstract void OnDeath();
	}
}
