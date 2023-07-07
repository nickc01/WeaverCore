using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore
{
	/// <summary>
	/// An object that contains a list of music tracks to be played, and what music snapshot should be applied
	/// </summary>
	[CreateAssetMenu(fileName = "MusicPack",menuName = "WeaverCore/Music Pack")]
	public class MusicPack : ScriptableObject
	{
		public static MusicPack None => WeaverAssets.LoadWeaverAsset<MusicPack>("None Music Pack");

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

		/// <summary>
		/// The delay before the pack gets applied
		/// </summary>
		public float delay = 0f;

		/// <summary>
		/// How long it will take for the music pack to transition
		/// </summary>
		public float snapshotTransitionTime = 0f;

		/// <summary>
		/// Should the snapshots be applied when the pack is applied?
		/// </summary>
		public bool applySnapshot = true;
	}
}
