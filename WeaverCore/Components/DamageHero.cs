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

		DamageHeroImplementation impl;


		void Awake()
		{
			impl = gameObject.AddComponent(ImplFinder.GetImplementationType<DamageHeroImplementation>()) as DamageHeroImplementation;
			impl.Damager = this;

			gameObject.layer = 11;
		}
	}
}
