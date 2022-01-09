using UnityEngine.Audio;
using WeaverCore.Implementations;

namespace WeaverCore
{
	/// <summary>
	/// Contains snapshots and mixers that are related to the Shade AudioMixer
	/// 
	/// These snapshots are used to enable and disable the shade sound effects in a scene
	/// </summary>
	public static class ShadeSounds
	{
		public enum SnapshotType
		{
			Away,
			Close
		}

		public enum GroupType
		{
			Master,
			Music,
			Shade
		}

		public static AudioMixerSnapshot GetSnapshot(SnapshotType type)
		{
			switch (type)
			{
				case SnapshotType.Away:
					return AwaySnapshot;
				case SnapshotType.Close:
					return CloseSnapshot;
				default:
					return null;
			}
		}

		public static AudioMixerGroup GetGroup(GroupType type)
		{
			switch (type)
			{
				case GroupType.Master:
					return MasterGroup;
				case GroupType.Music:
					return MusicGroup;
				case GroupType.Shade:
					return ShadeGroup;
				default:
					return null;
			}
		}

		public static AudioMixer ShadeMixer => AudioMixer_I.Instance.GetMixer("ShadeMixer");

		public static AudioMixerGroup MasterGroup => AudioMixer_I.Instance.GetGroupForMixer(ShadeMixer, "Master");
		public static AudioMixerGroup MusicGroup => AudioMixer_I.Instance.GetGroupForMixer(ShadeMixer, "Music");
		public static AudioMixerGroup ShadeGroup => AudioMixer_I.Instance.GetGroupForMixer(ShadeMixer, "Shade");

		public static AudioMixerSnapshot CloseSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(ShadeMixer, "Close");
		public static AudioMixerSnapshot AwaySnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(ShadeMixer, "Away");

		/// <summary>
		/// Applies a shade snapshot
		/// </summary>
		/// <param name="snapshot">The snapshot to apply</param>
		/// <param name="transitionTime">How long should it take to transition to the new snapshot</param>
		public static void ApplyShadeSounds(SnapshotType snapshot, float transitionTime)
		{
			GetSnapshot(snapshot).TransitionTo(transitionTime);
		}
	}
}
