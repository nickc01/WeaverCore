using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Enums
{
	/// <summary>
	/// Determines how music in a <see cref="MusicCue"/> will be synchronized
	/// </summary>
	public enum MusicSyncType
	{
		Implicit = 0,
		ExplicitOn = 1,
		ExplicitOff = 2,
	}
}
