using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Helpers;
using WeaverCore.Implementations;

namespace WeaverCore.Components
{
	public class DamageHero : MonoBehaviour
	{
		public int DamageDealt = 1;
		public HazardType HazardType = HazardType.Normal;
		public bool ShadowDashHazard = false;

		DamageHero_I impl;


		void Awake()
		{
			impl = gameObject.AddComponent(ImplFinder.GetImplementationType<DamageHero_I>()) as DamageHero_I;
			impl.Damager = this;

			gameObject.layer = 11;
		}
	}
}
