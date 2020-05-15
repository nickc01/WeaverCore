using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Interfaces;

namespace WeaverCore.Components.Base
{
	/// <summary>
	/// Used for default hit effects used already in the game
	/// </summary>
	public abstract class HitEffects : IHitEffects
	{
		public abstract void PlayHitEffect(float attackDirection);
	}
}
