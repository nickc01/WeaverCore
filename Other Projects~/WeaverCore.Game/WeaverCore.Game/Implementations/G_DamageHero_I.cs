using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
	public class G_DamageHero_I : DamageHero_I
	{
		DamageHero gameDamager;

		public override void Refresh()
		{
			gameDamager.damageDealt = Damager.enabled ? Damager.DamageDealt : 0;
			gameDamager.hazardType = (int)Damager.HazardType;
			gameDamager.shadowDashHazard = Damager.ShadowDashHazard;
		}

		void Start()
		{
			gameDamager = gameObject.AddComponent<DamageHero>();
			gameDamager.resetOnEnable = false;
			Refresh();
		}
	}
}
