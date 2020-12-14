using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Enums;

namespace WeaverCore.Core
{
	public class MusicPack : ScriptableObject
	{
		public AudioClip MainTrack;
		public MusicSyncType MainTrackSync;

		public AudioClip AltTrack;
		public MusicSyncType AltTrackSync;

		public AudioClip ActionTrack;
		public MusicSyncType ActionTrackSync;

		public AudioClip SubTrack;
		public MusicSyncType SubTrackSync;

		public AudioClip TensionTrack;
		public MusicSyncType TensionTrackSync;

		public AudioClip ExtraTrack;
		public MusicSyncType ExtraTrackSync;
	}
}
