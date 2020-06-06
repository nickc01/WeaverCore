using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
	public class GameDamageHeroImplementation : DamageHeroImplementation
	{
		DamageHero gameDamager;


		void Start()
		{
			gameDamager = gameObject.AddComponent<DamageHero>();
			gameDamager.resetOnEnable = false;
			UpdateDamager();
		}

		void Update()
		{
			UpdateDamager();
		}

		void UpdateDamager()
		{
			gameDamager.damageDealt = Damager.enabled ? Damager.DamageDealt : 0;
			gameDamager.hazardType = (int)Damager.HazardType;
			gameDamager.shadowDashHazard = Damager.ShadowDashHazard;
		}
	}
}
