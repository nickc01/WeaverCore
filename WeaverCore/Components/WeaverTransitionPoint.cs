using GlobalEnums;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WeaverCore.Assets.Components;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// An area in the game the player can transition to/from a scene
    /// </summary>
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
		private Atmos.SnapshotType m_AtmosSnapshot = Atmos.SnapshotType.atNone;

		[SerializeField]
		[Tooltip("Should the TransitionPoint change the enviro effects snapshot when the player touches it?")]
		bool enableEnviroSnapshot = false;
		[SerializeField]
		[Tooltip("The enviro effects snapshot that is applied when the player touches this transition point")]
		private EnviroEffects.SnapshotType m_EnviroSnapshot = EnviroEffects.SnapshotType.enCliffs;

		[SerializeField]
		[Tooltip("Should the TransitionPoint change the actor snapshot when the player touches it?")]
		bool enableActorSnapshot = true;
		[SerializeField]
		[Tooltip("The actor snapshot that is applied when the player touches this transition point")]
		private ActorSounds.SnapshotType m_ActorSnapshot = ActorSounds.SnapshotType.Off;

		[SerializeField]
		[Tooltip("Should the TransitionPoint change the music snapshot when the player touches it?")]
		bool enableMusicSnapshot = false;
		[SerializeField]
		[Tooltip("The music snapshot that is applied when the player touches this transition point")]
		private Music.SnapshotType m_MusicSnapshot = Music.SnapshotType.Normal;

		[SerializeField]
		[Tooltip("If set to true, the door will be functional and display text above it when the player is nearby")]
		bool enableDoorControl = true;

		public bool EnableDoorControl
		{
			get => enableDoorControl;
			set
			{
				if (enableDoorControl != value)
				{
					enableDoorControl = value;
					if (TryGetComponent<DoorControl>(out var door))
					{
						door.enabled = enableDoorControl && enabled;
					}
				}
			}
		}

		[OnHarmonyPatch]
		static void Init(HarmonyPatcher patcher)
		{
			patcher.Patch(typeof(TransitionPoint).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance), null, typeof(WeaverTransitionPoint).GetMethod(nameof(Start_Detour), BindingFlags.NonPublic | BindingFlags.Static));
			patcher.Patch(typeof(TransitionPoint).GetMethod("GetGatePosition", BindingFlags.Public | BindingFlags.Instance), typeof(WeaverTransitionPoint).GetMethod(nameof(Weaver_GetGatePosition), BindingFlags.NonPublic | BindingFlags.Static), null);
		}

		static void Start_Detour(TransitionPoint __instance)
		{
			if (__instance is WeaverTransitionPoint wtp)
			{
				wtp.Weaver_Start();
			}
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
            if (gameObject != null)
            {
                gameObject.tag = "TransitionGate";
                gameObject.layer = LayerMask.NameToLayer("Hero Detector");
            }
            //OnValidate();
            if (gateType == GatePosition.door && GetComponent<DoorControl>() == null)
            {
				var doorControl = gameObject.AddComponent<DoorControl>();
				doorControl.enabled = EnableDoorControl;

                doorControl.OnEnter.AddListener((knight_anim) =>
				{
					StartCoroutine(DoTransition(knight_anim));
				});
            }
		}

        private void OnEnable()
        {
            if (TryGetComponent<DoorControl>(out var door))
            {
				door.enabled = EnableDoorControl;
            }
        }

        private void OnDisable()
        {
			if (TryGetComponent<DoorControl>(out var door))
			{
				door.enabled = false;
			}
		}

#if UNITY_EDITOR
		private void Reset()
		{
			UnboundCoroutine.Start(SetMasks(gameObject));
		}

		private void OnValidate()
		{
			atmosSnapshot = enableAtmosSnapshot ? Atmos.GetSnapshot(m_AtmosSnapshot) : null;
			enviroSnapshot = enableEnviroSnapshot ? EnviroEffects.GetSnapshot(m_EnviroSnapshot) : null;
			actorSnapshot = enableActorSnapshot ? ActorSounds.GetSnapshot(m_ActorSnapshot) : null;
			musicSnapshot = enableMusicSnapshot ? Music.GetSnapshot(m_MusicSnapshot) : null;
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
#endif

		public IEnumerator DoTransition(string heroAnimation, float transitionTime = 0.3f)
        {
			var type = typeof(TransitionPoint);
			if (atmosSnapshot != null)
			{
				atmosSnapshot.TransitionTo(transitionTime);
			}
			if (enviroSnapshot != null)
			{
				enviroSnapshot.TransitionTo(transitionTime);
			}
			if (actorSnapshot != null)
			{
				actorSnapshot.TransitionTo(transitionTime);
			}
			if (musicSnapshot != null)
			{
				musicSnapshot.TransitionTo(transitionTime);
			}

            //TODO : SEND COMPASS EVENT

            if (!string.IsNullOrEmpty(heroAnimation))
            {
				HeroUtilities.PlayPlayerClip(heroAnimation);
            }

			type.GetField("activated", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, true);
			//activated = true;
			lastEntered = base.gameObject.name;

			var f = type.GetField("OnBeforeTransition",BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

			var onBeforeTransition = f.GetValue(this) as MulticastDelegate;

			//var onBeforeTransition = type.GetEvent("OnBeforeTransition").GetOtherMethods

			//onBeforeTransition.Invoke(this, null);
			if (onBeforeTransition != null)
			{
				onBeforeTransition.DynamicInvoke();
			}

			EventManager.SendEventToGameObject("DOOR ENTER", gameObject, gameObject);
			HeroController.instance.RelinquishControl();
			HeroController.instance.StopAnimationControl();
			PlayerData.instance.SetBool("disablePause", true);

			EventManager.SendEventToGameObject("FADE OUT", WeaverCamera.Instance.Cameras.mainCamera.gameObject, gameObject);

			yield return new WaitForSeconds(0.5f);

			PlayerData.instance.SetBool("isInvincible", true);

			GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
			{
				SceneName = targetScene,
				EntryGateName = entryPoint,
				HeroLeaveDirection = GetGatePosition(),
				EntryDelay = entryDelay,
				WaitForSceneTransitionCameraFade = true,
				//PreventCameraFadeOut = (customFadeFSM != null),
				Visualization = sceneLoadVisualization,
				AlwaysUnloadUnusedAssets = false,
				forceWaitFetch = false
			});
		}
	}

}