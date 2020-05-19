using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class HollowAudioImplementation : IImplementation
	{
		public abstract HollowPlayer.HollowAudioObject Play(AudioClip clip, Vector3 position, float volume, AudioChannel channel, bool autoPlay);
	}
}
