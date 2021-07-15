using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Implementations;
using WeaverCore.Enums;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace WeaverCore.Components
{
	public class PlayerDamager : DamageHero
	{
		public new HazardType hazardType
		{
			get => (HazardType)base.hazardType;
			set => base.hazardType = (int)value;
		}
	}
}

/*namespace WeaverCore.Components
{
	public class PlayerDamager : MonoBehaviour
	{
		[SerializeField]
		int damageDealt = 1;
		[SerializeField]
		HazardType hazardType = HazardType.Normal;
		[SerializeField]
		bool shadowDashHazard = false;


		DamageHero_I impl;

		public int DamageDealt
		{
			get
			{
				return damageDealt;
			}

			set
			{
				damageDealt = value;
				impl.Refresh();
			}
		}

		public HazardType HazardType
		{
			get
			{
				return hazardType;
			}

			set
			{
				hazardType = value;
				impl.Refresh();
			}
		}

		public bool ShadowDashHazard
		{
			get
			{
				return shadowDashHazard;
			}

			set
			{
				shadowDashHazard = value;
				impl.Refresh();
			}
		}

		void Awake()
		{
			if (impl == null)
			{
				impl = gameObject.AddComponent(ImplFinder.GetImplementationType<DamageHero_I>()) as DamageHero_I;
				impl.Damager = this;
			}
		}

		void OnValidate()
		{
			if (impl != null)
			{
				impl.Refresh();
			}
		}
	}
}*/
