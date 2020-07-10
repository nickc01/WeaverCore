using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class DamageHero_I : MonoBehaviour, IImplementation
	{
		[HideInInspector]
		public WeaverCore.Components.DamageHero Damager;

		public abstract void Refresh();
	}
}
