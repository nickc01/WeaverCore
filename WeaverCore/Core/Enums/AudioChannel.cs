using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Enums
{
	/// <summary>
	/// The type of audio channel the hollow knight sound should play on
	/// </summary>
	public enum AudioChannel
	{
		/// <summary>
		/// Plays the sound with no regard for any of the audio channels
		/// </summary>
		None,
		/// <summary>
		/// Plays on the master channel
		/// </summary>
		Master,
		/// <summary>
		/// Plays on the sound channel
		/// </summary>
		Sound,
		/// <summary>
		/// Plays on the music channel
		/// </summary>
		Music,
	}
}
