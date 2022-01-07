using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Settings
{
	/// <summary>
	/// Applied to a UI Element to determine whether it's visible in certain conditions
	/// </summary>
	[Flags]
	public enum EnabledType
	{
		/// <summary>
		/// Element is never visible
		/// </summary>
		Hidden,
		/// <summary>
		/// Element is only visible from within the main menu
		/// </summary>
		MenuOnly,
		/// <summary>
		/// Element is only visible from the pause screen in the game
		/// </summary>
		PauseOnly,
		/// <summary>
		/// Element is visible anywhere
		/// </summary>
		AlwaysVisible
	}
}
