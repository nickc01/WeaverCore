using System;
using System.Collections.Generic;
using UnityEngine.Audio;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;

namespace WeaverCore
{
    /// <summary>
    /// Contains snapshots and mixers that are related to the Atmos AudioMixer
    /// 
    /// These snapshots are used to change what atmosphere sounds are being played in a scene
    /// 
    /// See this table for more info on what groups/mixers do what : https://1drv.ms/x/s!Aj62egREH4PTxyT0dl7ijj9itSdV?e=SMNe12
    /// </summary>
    public static class Atmos
	{
		[Flags]
		public enum AtmosSources
		{
			None = 0,
			CaveWind = 1,
			SurfaceWind = 2,
			GrassWind = 4,
			SurfaceWindWhistling = 8,
			CaveNoises = 16,
			RainIndoor = 32,
			RainOutdoor = 64,
			GreenpathSounds = 128,
			FungusSounds = 256,
			FogCanyonSounds = 512,
			WaterwaysSounds = 1024,
			Waterfall = 2048,
			MineCrystals = 4096,
			MineMachinery = 8192,
			DeepnestSounds = 16384,
			MiscWind = 32768,
			Everything = 65535
		}

		public enum SnapshotType
		{
			atNone,
			atCave,
			atSurface,
			atSurfaceInterior,
			atSurfaceBasement,
			atSurfaceNook,
			atRainyIndoors,
			atRainyOutdoors,
			atDistantRain,
			atDistantRainRoom,
			atGreenpath,
			atQueensGardens,
			atFungus,
			atFogCanyon,
			atWaterwaysFlowing,
			atWaterways,
			atGreenpathInterior,
			atFogCanyonMinor,
			atMinesCrystal,
			atMinesMachinery,
			atDeepnest,
			atDeepnestQuiet,
			atWindTunnel,
			atMiscWind
		}

		public enum GroupType
		{
			Master,
			CaveNoises,
			CaveWind,
			Deepnest,
			FogCanyon,
			Fungus,
			GrassyWind,
			Greenpath,
			SurfaceWind1,
			SurfaceWind2,
			RainIndoor,
			RainOutdoor,
			Waterways,
			WaterfallMed,
			MinesCrystal,
			MinesMachinery,
			MiscWind
		}

		public static AudioMixerSnapshot GetSnapshot(SnapshotType type)
		{
			switch (type)
			{
				case SnapshotType.atNone:
					return NoneSnapshot;
				case SnapshotType.atCave:
					return CaveSnapshot;
				case SnapshotType.atSurface:
					return SurfaceSnapshot;
				case SnapshotType.atSurfaceInterior:
					return SurfaceInteriorSnapshot;
				case SnapshotType.atSurfaceBasement:
					return SurfaceBasementSnapshot;
				case SnapshotType.atSurfaceNook:
					return SurfaceNookSnapshot;
				case SnapshotType.atRainyIndoors:
					return RainyIndoorsSnapshot;
				case SnapshotType.atRainyOutdoors:
					return RainyOutdoorsSnapshot;
				case SnapshotType.atDistantRain:
					return DistantRainSnapshot;
				case SnapshotType.atDistantRainRoom:
					return DistantRainRoomSnapshot;
				case SnapshotType.atGreenpath:
					return GreenpathSnapshot;
				case SnapshotType.atQueensGardens:
					return QueensGardensSnapshot;
				case SnapshotType.atFungus:
					return FungusSnapshot;
				case SnapshotType.atFogCanyon:
					return FogCanyonSnapshot;
				case SnapshotType.atWaterwaysFlowing:
					return WaterwaysFlowingSnapshot;
				case SnapshotType.atWaterways:
					return WaterwaysSnapshot;
				case SnapshotType.atGreenpathInterior:
					return GreenpathInteriorSnapshot;
				case SnapshotType.atFogCanyonMinor:
					return FogCanyonMinorSnapshot;
				case SnapshotType.atMinesCrystal:
					return MinesCrystalSnapshot;
				case SnapshotType.atMinesMachinery:
					return MinesMachinerySnapshot;
				case SnapshotType.atDeepnest:
					return DeepnestSnapshot;
				case SnapshotType.atDeepnestQuiet:
					return DeepnestQuietSnapshot;
				case SnapshotType.atWindTunnel:
					return WindTunnelSnapshot;
				case SnapshotType.atMiscWind:
					return MiscWindSnapshot;
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
				case GroupType.CaveNoises:
					return CaveNoisesGroup;
				case GroupType.CaveWind:
					return CaveWindGroup;
				case GroupType.Deepnest:
					return DeepnestGroup;
				case GroupType.FogCanyon:
					return FogCanyonGroup;
				case GroupType.Fungus:
					return FungusGroup;
				case GroupType.GrassyWind:
					return GrassyWindGroup;
				case GroupType.Greenpath:
					return GreenpathGroup;
				case GroupType.SurfaceWind1:
					return SurfaceWind1Group;
				case GroupType.SurfaceWind2:
					return SurfaceWind2Group;
				case GroupType.RainIndoor:
					return RainIndoorGroup;
				case GroupType.RainOutdoor:
					return RainOutdoorGroup;
				case GroupType.Waterways:
					return WaterwaysGroup;
				case GroupType.WaterfallMed:
					return WaterfallMedGroup;
				case GroupType.MinesCrystal:
					return MinesCrystalGroup;
				case GroupType.MinesMachinery:
					return MinesMachineryGroup;
				case GroupType.MiscWind:
					return MiscWindGroup;
				default:
					return null;
			}
		}

		public static AtmosSources GetEnabledSourcesForSnapshot(SnapshotType snapshot)
		{
			return GetEnabledSourcesForSnapshot(GetSnapshot(snapshot));
		}

		static Dictionary<AudioMixerSnapshot, AtmosSources> sourcesCache = new Dictionary<AudioMixerSnapshot, AtmosSources>();

		public static AtmosSources GetEnabledSourcesForSnapshot(AudioMixerSnapshot snapshot)
		{
			if (sourcesCache == null)
			{
				sourcesCache = new Dictionary<AudioMixerSnapshot, AtmosSources>
				{
					{NoneSnapshot, AtmosSources.None},
					{CaveSnapshot, AtmosSources.CaveWind | AtmosSources.CaveNoises | AtmosSources.RainIndoor | AtmosSources.RainOutdoor},
					{SurfaceSnapshot, AtmosSources.SurfaceWind | AtmosSources.SurfaceWindWhistling | AtmosSources.RainIndoor | AtmosSources.RainOutdoor},
					{SurfaceInteriorSnapshot, AtmosSources.SurfaceWind | AtmosSources.SurfaceWindWhistling | AtmosSources.RainIndoor | AtmosSources.RainOutdoor},
					{SurfaceBasementSnapshot, AtmosSources.CaveWind | AtmosSources.SurfaceWind | AtmosSources.SurfaceWindWhistling | AtmosSources.RainIndoor | AtmosSources.RainOutdoor},
					{SurfaceNookSnapshot, AtmosSources.CaveWind | AtmosSources.SurfaceWind | AtmosSources.SurfaceWindWhistling | AtmosSources.RainIndoor | AtmosSources.RainOutdoor},
					{RainyIndoorsSnapshot, AtmosSources.SurfaceWind | AtmosSources.SurfaceWindWhistling | AtmosSources.RainIndoor | AtmosSources.RainOutdoor | AtmosSources.GreenpathSounds},
					{RainyOutdoorsSnapshot, AtmosSources.SurfaceWind | AtmosSources.SurfaceWindWhistling | AtmosSources.RainIndoor | AtmosSources.RainOutdoor},
					{DistantRainSnapshot, AtmosSources.CaveWind | AtmosSources.CaveNoises | AtmosSources.RainIndoor | AtmosSources.RainOutdoor},
					{DistantRainRoomSnapshot, AtmosSources.RainIndoor},
					{GreenpathSnapshot, AtmosSources.GreenpathSounds},
					{QueensGardensSnapshot, AtmosSources.GreenpathSounds},
					{FungusSnapshot, AtmosSources.CaveWind | AtmosSources.FungusSounds},
					{FogCanyonSnapshot, AtmosSources.FogCanyonSounds},
					{WaterwaysFlowingSnapshot, AtmosSources.WaterwaysSounds | AtmosSources.Waterfall},
					{WaterwaysSnapshot, AtmosSources.WaterwaysSounds},
					{GreenpathInteriorSnapshot, AtmosSources.CaveWind | AtmosSources.GreenpathSounds},
					{FogCanyonMinorSnapshot, AtmosSources.CaveWind | AtmosSources.CaveNoises | AtmosSources.FogCanyonSounds},
					{MinesCrystalSnapshot, AtmosSources.CaveWind | AtmosSources.MineCrystals},
					{MinesMachinerySnapshot, AtmosSources.CaveWind | AtmosSources.MineCrystals | AtmosSources.MineMachinery},
					{DeepnestSnapshot, AtmosSources.RainIndoor | AtmosSources.RainOutdoor | AtmosSources.DeepnestSounds},
					{DeepnestQuietSnapshot, AtmosSources.RainIndoor | AtmosSources.RainOutdoor | AtmosSources.DeepnestSounds},
					{WindTunnelSnapshot, AtmosSources.SurfaceWind | AtmosSources.GrassWind | AtmosSources.SurfaceWindWhistling | AtmosSources.RainIndoor | AtmosSources.RainOutdoor},
					{MiscWindSnapshot, AtmosSources.MiscWind}
				};
			}
			if (sourcesCache.TryGetValue(snapshot,out var result))
			{
				return result;
			}
			else
			{
				return AtmosSources.Everything;
			}
		}

		public static AudioMixer AtmosMixer => AudioMixer_I.Instance.GetMixer("Atmos");

		public static AudioMixerGroup MasterGroup => AudioMixer_I.Instance.GetGroupForMixer(AtmosMixer, "Master");
		public static AudioMixerGroup CaveNoisesGroup => AudioMixer_I.Instance.GetGroupForMixer(AtmosMixer, "Cave Noises");
		public static AudioMixerGroup CaveWindGroup => AudioMixer_I.Instance.GetGroupForMixer(AtmosMixer, "Cave Wind");
		public static AudioMixerGroup DeepnestGroup => AudioMixer_I.Instance.GetGroupForMixer(AtmosMixer, "Deepnest");
		public static AudioMixerGroup FogCanyonGroup => AudioMixer_I.Instance.GetGroupForMixer(AtmosMixer, "Fog Canyon");
		public static AudioMixerGroup FungusGroup => AudioMixer_I.Instance.GetGroupForMixer(AtmosMixer, "Fungus");
		public static AudioMixerGroup GrassyWindGroup => AudioMixer_I.Instance.GetGroupForMixer(AtmosMixer, "Grassy Wind");

		public static AudioMixerGroup GreenpathGroup => AudioMixer_I.Instance.GetGroupForMixer(AtmosMixer, "Greenpath");
		public static AudioMixerGroup SurfaceWind1Group => AudioMixer_I.Instance.GetGroupForMixer(AtmosMixer, "Surface Wind 1");
		public static AudioMixerGroup SurfaceWind2Group => AudioMixer_I.Instance.GetGroupForMixer(AtmosMixer, "Surface Wind 2");
		public static AudioMixerGroup RainIndoorGroup => AudioMixer_I.Instance.GetGroupForMixer(AtmosMixer, "Rain Indoor");
		public static AudioMixerGroup RainOutdoorGroup => AudioMixer_I.Instance.GetGroupForMixer(AtmosMixer, "Rain Outdoor");
		public static AudioMixerGroup WaterwaysGroup => AudioMixer_I.Instance.GetGroupForMixer(AtmosMixer, "Waterways");
		public static AudioMixerGroup WaterfallMedGroup => AudioMixer_I.Instance.GetGroupForMixer(AtmosMixer, "Waterfall Med");
		public static AudioMixerGroup MinesCrystalGroup => AudioMixer_I.Instance.GetGroupForMixer(AtmosMixer, "Mines Crystal");
		public static AudioMixerGroup MinesMachineryGroup => AudioMixer_I.Instance.GetGroupForMixer(AtmosMixer, "Mines Machinery");
		public static AudioMixerGroup MiscWindGroup => AudioMixer_I.Instance.GetGroupForMixer(AtmosMixer, "Misc Wind");


		public static AudioMixerSnapshot NoneSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at None");
		public static AudioMixerSnapshot CaveSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Cave");
		public static AudioMixerSnapshot SurfaceSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Surface");
		public static AudioMixerSnapshot SurfaceInteriorSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Surface Interior");
		public static AudioMixerSnapshot SurfaceBasementSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Surface Basement");
		public static AudioMixerSnapshot SurfaceNookSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Surface Nook");
		public static AudioMixerSnapshot RainyIndoorsSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Rainy Indoors");
		public static AudioMixerSnapshot RainyOutdoorsSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Rainy Outdoors");
		public static AudioMixerSnapshot DistantRainSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Distant Rain");
		public static AudioMixerSnapshot DistantRainRoomSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Distant Rain Room");
		public static AudioMixerSnapshot GreenpathSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Greenpath");
		public static AudioMixerSnapshot QueensGardensSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Queens Gardens");
		public static AudioMixerSnapshot FungusSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Fungus");
		public static AudioMixerSnapshot FogCanyonSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Fog Canyon");
		public static AudioMixerSnapshot WaterwaysFlowingSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Waterways Flowing");
		public static AudioMixerSnapshot WaterwaysSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Waterways");
		public static AudioMixerSnapshot GreenpathInteriorSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Greenpath Interior");
		public static AudioMixerSnapshot FogCanyonMinorSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Fog Canyon Minor");
		public static AudioMixerSnapshot MinesCrystalSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Mines Crystal");
		public static AudioMixerSnapshot MinesMachinerySnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Mines Machinery");
		public static AudioMixerSnapshot DeepnestSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Deepnest");
		public static AudioMixerSnapshot DeepnestQuietSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Deepnest Quiet");
		public static AudioMixerSnapshot WindTunnelSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Wind Tunnel");
		public static AudioMixerSnapshot MiscWindSnapshot => AudioMixer_I.Instance.GetSnapshotForMixer(AtmosMixer, "at Misc Wind");

		/// <summary>
		/// Applies an AtmosPack to change the atmosphere sounds
		/// </summary>
		/// <param name="pack">The pack to be applied</param>
		/// <param name="transitionTime">The time to transition to the new atmos pack</param>
		public static void ApplyAtmosPack(AtmosPack pack, float transitionTime)
		{
			ApplyAtmosSnapshot(pack.Snapshot, transitionTime, GetEnabledSourcesForSnapshot(pack.Snapshot));
		}

		/// <summary>
		/// Applies an AtmosPack to change the atmosphere sounds
		/// </summary>
		/// <param name="pack">The pack to be applied</param>
		/// <param name="transitionTime">The time to transition to the new atmos pack</param>
		/// <param name="enabledSources">A list of sources to be enabled</param>
		public static void ApplyAtmosPack(AtmosPack pack, float transitionTime, AtmosSources enabledSources)
		{
			ApplyAtmosSnapshot(pack.Snapshot, transitionTime, enabledSources);
		}

		/// <summary>
		/// Applies an Atmos Snapshot to change the atmosphere sounds
		/// </summary>
		/// <param name="snapshot">The snapshot to be applied</param>
		/// <param name="transitionTime">The time to transition to the new Atmos snapshot</param>
		public static void ApplyAtmosSnapshot(SnapshotType snapshot, float transitionTime)
		{
			ApplyAtmosSnapshot(snapshot, transitionTime,GetEnabledSourcesForSnapshot(snapshot));
		}

		/// <summary>
		/// Applies an Atmos Snapshot to change the atmosphere sounds
		/// </summary>
		/// <param name="snapshot">The snapshot to be applied</param>
		/// <param name="transitionTime">The time to transition to the new Atmos snapshot</param>
		/// <param name="enabledSources">A list of sources to be enabled</param>
		public static void ApplyAtmosSnapshot(SnapshotType snapshot, float transitionTime, AtmosSources enabledSources)
		{
			AudioMixer_I.Instance.ApplyAtmosSnapshot(snapshot, transitionTime, enabledSources);
		}
	}
}
