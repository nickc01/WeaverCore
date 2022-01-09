using System;

namespace GlobalEnums
{
	// Token: 0x02000B68 RID: 2920
	public enum GatePosition
	{
		/// <summary>
		/// The transition point is located at the top of the level. The player will spawn travelling downwards
		/// </summary>
		top,

		/// <summary>
		/// The transition point is located at the right side of the level. The player will spawn travelling leftwards
		/// </summary>
		right,

		/// <summary>
		/// The transition point is located at the left side of the level. The player will spawn travelling rightwards
		/// </summary>
		left,

		/// <summary>
		/// The transition point is located at the bottom of the level. The player will spawn travelling upwards
		/// </summary>
		bottom,

		/// <summary>
		/// The transition point will act as a doorway. The player will spawn leaving the doorway
		/// </summary>
		door,

		/// <summary>
		/// Don't use this
		/// </summary>
		unknown
	}
}
