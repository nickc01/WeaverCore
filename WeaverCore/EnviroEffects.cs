using UnityEngine.Audio;
using WeaverCore.Implementations;

namespace WeaverCore
{
	/// <summary>
	/// Contains snapshots and mixers that are related to the EnviroEffects AudioMixer
	/// 
	/// These snapshots are used to change what enviroment sounds are being played in a scene
	/// 
	/// See this table for more info on what groups/mixers do what : https://1drv.ms/x/s!Aj62egREH4PTxyIkYuCSF6zX5zsp?e=JpJksw
	/// </summary>
	public static class EnviroEffects
	{
		public enum SnapshotType
		{
			enCave,
			enSpa,
			enCliffs,
			enRoom,
			enArena,
			enSewerpipe,
			enFogCanyon,
			enDream,
			enSilent
		}

		public enum GroupType
		{
			Master,
			Actor,
			Atmos
		}

		public static AudioMixerSnapshot GetSnapshot(SnapshotType type)
		{
			switch (type)
			{
				case SnapshotType.enCave:
					return enCaveSnapshot;
				case SnapshotType.enSpa:
					return enSpaSnapshot;
				case SnapshotType.enCliffs:
					return enCliffsSnapshot;
				case SnapshotType.enRoom:
					return enRoomSnapshot;
				case SnapshotType.enArena:
					return enArenaSnapshot;
				case SnapshotType.enSewerpipe:
					return enSewerpipeSnapshot;
				case SnapshotType.enFogCanyon:
					return enFogCanyonSnapshot;
				case SnapshotType.enDream:
					return enDreamSnapshot;
				case SnapshotType.enSilent:
					return enSilentSnapshot;
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
				case GroupType.Actor:
					return ActorsGroup;
				case GroupType.Atmos:
					return AtmosGroup;
				default:
					return null;
			}
		}

		public static AudioMixer EnviroEffectsMixer => AudioMixer_I.Instance.GetMixer("EnviroEffects");

		public static AudioMixerGroup MasterGroup => AudioMixer_I.Instance.GetGroupForMixer(EnviroEffectsMixer, "Master");
		public static AudioMixerGroup ActorsGroup => AudioMixer_I.Instance.GetGroupForMixer(EnviroEffectsMixer, "Actors");
		public static AudioMixerGroup AtmosGroup => AudioMixer_I.Instance.GetGroupForMixer(EnviroEffectsMixer, "Atmos");

		public static AudioMixerSnapshot enCaveSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(EnviroEffectsMixer, "en Cave");
		public static AudioMixerSnapshot enSpaSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(EnviroEffectsMixer, "en Spa");
		public static AudioMixerSnapshot enCliffsSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(EnviroEffectsMixer, "en Cliffs");
		public static AudioMixerSnapshot enRoomSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(EnviroEffectsMixer, "en Room");
		public static AudioMixerSnapshot enArenaSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(EnviroEffectsMixer, "en Arena");
		public static AudioMixerSnapshot enSewerpipeSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(EnviroEffectsMixer, "en Sewerpipe");
		public static AudioMixerSnapshot enFogCanyonSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(EnviroEffectsMixer, "en Fog Canyon");
		public static AudioMixerSnapshot enDreamSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(EnviroEffectsMixer, "en Dream");
		public static AudioMixerSnapshot enSilentSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(EnviroEffectsMixer, "en Silent");

		/// <summary>
		/// Applies an EnviroEffects snapshot to change what enviroment sounds are being played
		/// </summary>
		/// <param name="snapshot">The snapshot to be applied</param>
		/// <param name="transitionTime">How long it should take to transition to the new snapshot</param>
		public static void ApplyEnviroEffectsSnapshot(SnapshotType snapshot, float transitionTime)
		{
			GetSnapshot(snapshot).TransitionTo(transitionTime);
		}
	}
}
