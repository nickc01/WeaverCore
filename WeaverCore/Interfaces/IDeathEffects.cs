using System;

namespace WeaverCore.Interfaces
{
	/// <summary>
	/// Interface for Enemy Death Effects
	/// </summary>
	public interface IDeathEffects
	{
		/// <summary>
		/// Used to play the death effects
		/// </summary>
		/// <param name="finalBlow">The final blow on the enemy</param>
		void PlayDeathEffects(HitInfo finalBlow);

		/// <summary>
		/// Used to only emit the particle effects
		/// </summary>
		void EmitEffects();

		/// <summary>
		/// Used to only emit the sound effects
		/// </summary>
		void EmitSounds();
	}
}