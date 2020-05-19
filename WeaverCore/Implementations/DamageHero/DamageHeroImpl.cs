using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class DamageHeroImplementation : MonoBehaviour, IImplementation
	{
		[HideInInspector]
		public DamageHero Damager;
	}
}
