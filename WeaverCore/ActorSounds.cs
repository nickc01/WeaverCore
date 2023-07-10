using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Audio;
using WeaverCore.Implementations;

namespace WeaverCore
{

    /// <summary>
    /// Contains groups and snapshots that are related to the Actors AudioMixer
    /// 
    /// See this table for more info on what groups/mixers do what : https://1drv.ms/x/s!Aj62egREH4PTxyBaaVb5nI-NmXiX?e=bXtNVc
    /// </summary>
    public static class ActorSounds
	{
		public enum SnapshotType
		{
			On,
			Off
		}

		public enum GroupType
		{
			Master,
			Actors,
			ActorsFlange
		}

		public static AudioMixerSnapshot GetSnapshot(SnapshotType type)
		{
			switch (type)
			{
				case SnapshotType.On:
					return OnSnapshot;
				case SnapshotType.Off:
					return OffSnapshot;
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
				case GroupType.Actors:
					return ActorsGroup;
				case GroupType.ActorsFlange:
					return ActorsFlangeGroup;
				default:
					return null;
			}
		}

		public static AudioMixer ActorsMixer => AudioMixer_I.Instance.GetMixer("Actors");

		public static AudioMixerGroup MasterGroup => AudioMixer_I.Instance.GetGroupForMixer(ActorsMixer, "Master");
		public static AudioMixerGroup ActorsGroup => AudioMixer_I.Instance.GetGroupForMixer(ActorsMixer, "Actors");
		public static AudioMixerGroup ActorsFlangeGroup => AudioMixer_I.Instance.GetGroupForMixer(ActorsMixer, "Actors Flange");

		public static AudioMixerSnapshot OffSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(ActorsMixer, "Off");
		public static AudioMixerSnapshot OnSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(ActorsMixer, "On");

		/// <summary>
		/// Applies a specific actor snapshot
		/// </summary>
		/// <param name="snapshot">The snapshot to apply</param>
		/// <param name="transitionTime">How long it will take to transition to the new snapshot</param>
		public static void ApplyActorSounds(SnapshotType snapshot, float transitionTime)
		{
			GetSnapshot(snapshot).TransitionTo(transitionTime);
		}
	}
}
