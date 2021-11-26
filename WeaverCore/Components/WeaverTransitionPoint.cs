using GlobalEnums;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WeaverCore;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
	public class WeaverTransitionPoint : TransitionPoint
	{
		[Header("Gate Type")]
		[SerializeField]
		[Tooltip(@"The type of gate this transition point is. 
top -> The transition point is located at the top of the level. The player will spawn travelling downwards

right -> The transition point is located at the right side of the level. The player will spawn travelling leftwards

left -> The transition point is located at the left side of the level. The player will spawn travelling rightwards

bottom -> The transition point is located at the bottom of the level. The player will spawn travelling upwards

door -> The transition point will act as a doorway. The player will spawn leaving the doorway

unknown -> Don't use")]
		GatePosition gateType;
		[Header("Audio Snapshots")]
		[Space]
		[SerializeField]
		[Tooltip("Should the TransitionPoint change the atmos snapshot when the player touches it?")]
		bool enableAtmosSnapshot = false;
		[SerializeField]
		[Tooltip("The atmos snapshot that is applied when the player touches this transition point")]
		private Atmos.SnapshotType AtmosSnapshot = Atmos.SnapshotType.atNone;

		[SerializeField]
		[Tooltip("Should the TransitionPoint change the enviro effects snapshot when the player touches it?")]
		bool enableEnviroSnapshot = false;
		[SerializeField]
		[Tooltip("The enviro effects snapshot that is applied when the player touches this transition point")]
		private EnviroEffects.SnapshotType EnviroSnapshot = EnviroEffects.SnapshotType.enCliffs;

		[SerializeField]
		[Tooltip("Should the TransitionPoint change the actor snapshot when the player touches it?")]
		bool enableActorSnapshot = true;
		[SerializeField]
		[Tooltip("The actor snapshot that is applied when the player touches this transition point")]
		private ActorSounds.SnapshotType ActorSnapshot = ActorSounds.SnapshotType.Off;

		[SerializeField]
		[Tooltip("Should the TransitionPoint change the music snapshot when the player touches it?")]
		bool enableMusicSnapshot = false;
		[SerializeField]
		[Tooltip("The music snapshot that is applied when the player touches this transition point")]
		private Music.SnapshotType MusicSnapshot = Music.SnapshotType.Normal;

		[OnHarmonyPatch]
		static void Init(HarmonyPatcher patcher)
		{
			//Debug.Log("Patching Transition Points");
			patcher.Patch(typeof(TransitionPoint).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance), null, typeof(WeaverTransitionPoint).GetMethod("Start_Detour", BindingFlags.NonPublic | BindingFlags.Static));
			patcher.Patch(typeof(TransitionPoint).GetMethod("GetGatePosition", BindingFlags.Public | BindingFlags.Instance), typeof(WeaverTransitionPoint).GetMethod("Weaver_GetGatePosition", BindingFlags.NonPublic | BindingFlags.Static), null);
			//Debug.Log("Patching Transition Points DONE");
			//new Detour(typeof(TransitionPoint).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance), typeof(WeaverTransitionPoint).GetMethod("Start_Detour",BindingFlags.NonPublic | BindingFlags.Static));
			//new Detour(typeof(TransitionPoint).GetMethod("GetGatePosition", BindingFlags.Public | BindingFlags.Instance), typeof(WeaverTransitionPoint).GetMethod("Weaver_GetGatePosition", BindingFlags.NonPublic | BindingFlags.Static));
		}

		static void Start_Detour(TransitionPoint __instance)
		{
			//Debug.LogError("A");
			//orig(self);
			//Debug.LogError("B");
			if (__instance is WeaverTransitionPoint wtp)
			{
				//Debug.LogError("C");
				wtp.Weaver_Start();
				//Debug.LogError("D");
			}
			//Debug.LogError("E");
		}

		static bool Weaver_GetGatePosition(TransitionPoint __instance, ref GatePosition __result)
		{
			if (__instance is WeaverTransitionPoint wtp)
			{
				__result = wtp.gateType;
				return false;
			}
			else
			{
				return true;
			}
		}

		void Weaver_Start()
		{
			OnValidate();
		}

		private void Reset()
		{
			UnboundCoroutine.Start(SetMasks(gameObject));
		}

		private void OnValidate()
		{
			atmosSnapshot = enableAtmosSnapshot ? Atmos.GetSnapshot(AtmosSnapshot) : null;
			enviroSnapshot = enableEnviroSnapshot ? EnviroEffects.GetSnapshot(EnviroSnapshot) : null;
			actorSnapshot = enableActorSnapshot ? ActorSounds.GetSnapshot(ActorSnapshot) : null;
			musicSnapshot = enableMusicSnapshot ? Music.GetSnapshot(MusicSnapshot) : null;
			UnboundCoroutine.Start(SetMasks(gameObject));
		}

		static IEnumerator SetMasks(GameObject gameObject)
		{
			yield return null;
			if (gameObject != null)
			{
				gameObject.tag = "TransitionGate";
				gameObject.layer = LayerMask.NameToLayer("Hero Detector");
			}
		}
	}

}