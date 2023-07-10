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
using GlobalEnums;

namespace WeaverCore.Components
{
    /// <summary>
    /// This component causes the player to take damage when colliding with it
    /// </summary>
    public class PlayerDamager : DamageHero
	{
		public new HazardType hazardType
		{
			get => (HazardType)base.hazardType;
			set => base.hazardType = (int)value;
		}
	}
}
