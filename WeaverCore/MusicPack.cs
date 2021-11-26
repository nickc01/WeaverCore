using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Enums;

namespace WeaverCore
{

	[CreateAssetMenu(fileName = "MusicPack",menuName = "Music Pack")]
	public class MusicPack : ScriptableObject
	{
		public AudioClip MainTrack;
		public MusicSyncType MainTrackSync;

		public AudioClip MainAltTrack;
		public MusicSyncType MainAltTrackSync;

		public AudioClip ActionTrack;
		public MusicSyncType ActionTrackSync;

		public AudioClip SubTrack;
		public MusicSyncType SubTrackSync;

		public AudioClip TensionTrack;
		public MusicSyncType TensionTrackSync;

		public AudioClip ExtraTrack;
		public MusicSyncType ExtraTrackSync;

		public Music.SnapshotType Snapshot;

		public float delay = 0f;
		public float snapshotTransitionTime = 0f;
		public bool applySnapshot = true;
	}
}
