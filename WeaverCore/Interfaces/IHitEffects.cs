using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Interfaces
{
	/// <summary>
	/// Interface for Enemy hit effects
	/// </summary>
	public interface IHitEffects
	{
		/// <summary>
		/// Plays the enemy's hit effects
		/// </summary>
		/// <param name="hit">The hit on the enemy</param>
		/// <param name="effectsOffset">An offset applied to the effects</param>
		void PlayHitEffect(HitInfo hit, Vector3 effectsOffset = default(Vector3));
	}
}
