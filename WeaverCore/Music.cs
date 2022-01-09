using System.Linq;
using System.Text;
using UnityEngine.Audio;
using WeaverCore.Implementations;

namespace WeaverCore
{
	/// <summary>
	/// Contains snapshots and mixers that are related to the Music AudioMixer
	/// 
	/// These snapshots are used to change what music channels are being played in a scene
	/// 
	/// See this table for more info on what groups/mixers do what : https://1drv.ms/x/s!Aj62egREH4PTxx1MpsfuioqJtSCH?e=7kmAV0
	/// </summary>
	public static class Music
	{
		public enum SnapshotType
		{
			Normal,
			NormalAlt,
			NormalSoft,
			NormalSofter,
			NormalFlange,
			NormalFlangier,
			Action,
			ActionAndSub,
			SubArea,
			Silent,
			SilentFlange,
			Off,
			TensionOnly,
			NormalGramaphone,
			ActionOnly,
			MainOnly,
			HKDecline2,
			HKDecline3,
			HKDecline4,
			HKDecline5,
			HKDecline6
		}

		public enum GroupType
		{
			Master,
			Main,
			MainAlt,
			Action,
			Sub,
			Tension,
			Extra
		}

		public static AudioMixerSnapshot GetSnapshot(SnapshotType type)
		{
			switch (type)
			{
				case SnapshotType.Normal:
					return NormalSnapshot;

				case SnapshotType.NormalAlt:
					return NormalAltSnapshot;

				case SnapshotType.NormalSoft:
					return NormalSoftSnapshot;

				case SnapshotType.NormalSofter:
					return NormalSofterSnapshot;

				case SnapshotType.NormalFlange:
					return NormalFlangeSnapshot;

				case SnapshotType.NormalFlangier:
					return NormalFlangierSnapshot;

				case SnapshotType.Action:
					return ActionSnapshot;

				case SnapshotType.ActionAndSub:
					return ActionAndSubSnapshot;

				case SnapshotType.SubArea:
					return SubAreaSnapshot;

				case SnapshotType.Silent:
					return SilentSnapshot;

				case SnapshotType.SilentFlange:
					return SilentFlangeSnapshot;

				case SnapshotType.Off:
					return OffSnapshot;

				case SnapshotType.TensionOnly:
					return TensionOnlySnapshot;

				case SnapshotType.NormalGramaphone:
					return NormalGramaphoneSnapshot;

				case SnapshotType.ActionOnly:
					return ActionOnlySnapshot;

				case SnapshotType.MainOnly:
					return MainOnlySnapshot;

				case SnapshotType.HKDecline2:
					return HKDecline2Snapshot;

				case SnapshotType.HKDecline3:
					return HKDecline3Snapshot;

				case SnapshotType.HKDecline4:
					return HKDecline4Snapshot;

				case SnapshotType.HKDecline5:
					return HKDecline5Snapshot;

				case SnapshotType.HKDecline6:
					return HKDecline6Snapshot;

				default:
					return NormalSnapshot;
			}
		}

		public static AudioMixerGroup GetGroup(GroupType type)
		{
			switch (type)
			{
				case GroupType.Master:
					return MasterGroup;
				case GroupType.Action:
					return ActionGroup;
				case GroupType.Extra:
					return ExtraGroup;
				case GroupType.Main:
					return MainAltGroup;
				case GroupType.MainAlt:
					return MainAltGroup;
				case GroupType.Sub:
					return SubGroup;
				case GroupType.Tension:
					return TensionGroup;
				default:
					return MasterGroup;
			}
		}

		public static AudioMixer MusicMixer => AudioMixer_I.Instance.GetMixer("Music");

		public static AudioMixerGroup MasterGroup => AudioMixer_I.Instance.GetGroupForMixer(MusicMixer, "Master");
		public static AudioMixerGroup ActionGroup => AudioMixer_I.Instance.GetGroupForMixer(MusicMixer, "Action");
		public static AudioMixerGroup ExtraGroup => AudioMixer_I.Instance.GetGroupForMixer(MusicMixer, "Extra");
		public static AudioMixerGroup MainGroup => AudioMixer_I.Instance.GetGroupForMixer(MusicMixer, "Main");
		public static AudioMixerGroup MainAltGroup => AudioMixer_I.Instance.GetGroupForMixer(MusicMixer, "Main Alt");
		public static AudioMixerGroup SubGroup => AudioMixer_I.Instance.GetGroupForMixer(MusicMixer, "Sub");
		public static AudioMixerGroup TensionGroup => AudioMixer_I.Instance.GetGroupForMixer(MusicMixer, "Tension");


		public static AudioMixerSnapshot NormalSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(MusicMixer, "Normal");
		public static AudioMixerSnapshot NormalAltSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(MusicMixer, "Normal Alt");
		public static AudioMixerSnapshot NormalSoftSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(MusicMixer, "Normal Soft");
		public static AudioMixerSnapshot NormalSofterSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(MusicMixer, "Normal Softer");
		public static AudioMixerSnapshot NormalFlangeSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(MusicMixer, "Normal Flange");
		public static AudioMixerSnapshot NormalFlangierSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(MusicMixer, "Normal Flangier");
		public static AudioMixerSnapshot ActionSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(MusicMixer, "Action");
		public static AudioMixerSnapshot ActionAndSubSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(MusicMixer, "Action and Sub");
		public static AudioMixerSnapshot SubAreaSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(MusicMixer, "Sub Area");
		public static AudioMixerSnapshot SilentSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(MusicMixer, "Silent");
		public static AudioMixerSnapshot SilentFlangeSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(MusicMixer, "Silent Flange");
		public static AudioMixerSnapshot OffSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(MusicMixer, "Off");
		public static AudioMixerSnapshot TensionOnlySnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(MusicMixer, "Tension Only");
		public static AudioMixerSnapshot NormalGramaphoneSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(MusicMixer, "Normal Gramaphone");
		public static AudioMixerSnapshot ActionOnlySnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(MusicMixer, "Action Only");
		public static AudioMixerSnapshot MainOnlySnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(MusicMixer, "Main Only");
		public static AudioMixerSnapshot HKDecline2Snapshot => AudioMixer_I.Instance.GetSnapshotForMixer(MusicMixer, "HK Decline 2");
		public static AudioMixerSnapshot HKDecline3Snapshot => AudioMixer_I.Instance.GetSnapshotForMixer(MusicMixer, "HK Decline 3");
		public static AudioMixerSnapshot HKDecline4Snapshot => AudioMixer_I.Instance.GetSnapshotForMixer(MusicMixer, "HK Decline 4");
		public static AudioMixerSnapshot HKDecline5Snapshot => AudioMixer_I.Instance.GetSnapshotForMixer(MusicMixer, "HK Decline 5");
		public static AudioMixerSnapshot HKDecline6Snapshot => AudioMixer_I.Instance.GetSnapshotForMixer(MusicMixer, "HK Decline 6");

		/// <summary>
		/// Applies a music pack to change what music channels are being played
		/// </summary>
		/// <param name="pack">The pack to be applied</param>
		public static void PlayMusicPack(MusicPack pack)
		{
			PlayMusicPack(pack, pack.delay, pack.snapshotTransitionTime, pack.applySnapshot);
		}

		/// <summary>
		/// Applies a music pack to change what music channels are being played
		/// </summary>
		/// <param name="pack">The pack to be applied</param>
		/// <param name="delayTime">The delay before the music pack is applied</param>
		/// <param name="snapshotTransitionTime">The time it will take to transition to the new snapshots</param>
		/// <param name="applySnapshot">Should the snapshots in the music pack also be applied?</param>
		public static void PlayMusicPack(MusicPack pack, float delayTime, float snapshotTransitionTime, bool applySnapshot = true)
		{
			AudioMixer_I.Instance.PlayMusicPack(pack, delayTime, snapshotTransitionTime, applySnapshot);
		}

		/// <summary>
		/// Applies a music snapshot to change what music channels are being played
		/// </summary>
		/// <param name="snapshot">The snapshot to be applied</param>
		/// <param name="delayTime">The delay before the music pack is applied</param>
		/// <param name="transitionTime">The time it will take to transition to the new snapshots</param>
		public static void ApplyMusicSnapshot(SnapshotType snapshot, float delayTime, float transitionTime)
		{
			ApplyMusicSnapshot(GetSnapshot(snapshot), delayTime, transitionTime);
		}

		/// <summary>
		/// Applies a snapshot to change what music channels are being played
		/// </summary>
		/// <param name="snapshot">The snapshot to be applied</param>
		/// <param name="delayTime">The delay before the music pack is applied</param>
		/// <param name="transitionTime">The time it will take to transition to the new snapshots</param>
		public static void ApplyMusicSnapshot(AudioMixerSnapshot snapshot, float delayTime, float transitionTime)
		{
			AudioMixer_I.Instance.ApplyMusicSnapshot(snapshot, delayTime, transitionTime);
		}
	}
}
