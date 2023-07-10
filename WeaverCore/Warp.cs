using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Utilities;

namespace WeaverCore
{
	/// <summary>
	/// Main class for dream warping the player to a new scene
	/// </summary>
	public static class Warp
	{
		/// <summary>
		/// Is the player currently dream warping?
		/// </summary>
		public static bool Warping { get; private set; }

		/// <summary>
		/// Contains customizable options for warping the player
		/// </summary>
		public class DreamWarpOptions
		{
			/// <summary>
			/// The sound that is played when the player is warping from a scene
			/// </summary>
			public AudioClip DreamWarpSound;

			/// <summary>
			/// The sound that is played when the player arrives at the new scene
			/// </summary>
			public AudioClip DreamArriveSound;

			/// <summary>
			/// The particles that are emitted when the player is warping from a scene
			/// </summary>
			public GameObject DreamParticles;

			/// <summary>
			/// The particles that are emitted when the player arrives at the new scene
			/// </summary>
			public GameObject DreamArriveParticles;
		}

		static DreamWarpOptions _dreamWarpDefaults;

		/// <summary>
		/// The default customization options when warping the player
		/// </summary>
		public static DreamWarpOptions DreamWarpDefaults
		{
			get
			{
				if (_dreamWarpDefaults == null)
				{
					_dreamWarpDefaults = new DreamWarpOptions
					{
						DreamWarpSound = WeaverAssets.LoadWeaverAsset<AudioClip>("Dream Enter"),
						DreamArriveSound = WeaverAssets.LoadWeaverAsset<AudioClip>("Enter Dream World"),
						DreamParticles = WeaverAssets.LoadWeaverAsset<GameObject>("Dream Particles"),
						DreamArriveParticles = WeaverAssets.LoadWeaverAsset<GameObject>("Dream Arrive Particles")
					};
				}
				return _dreamWarpDefaults;
			}
		}

		/// <summary>
		/// Warps the player to a new scene and plays the dream nail effects when doing so
		/// </summary>
		/// <param name="position">The position the effects should take place at</param>
		/// <param name="destinationScene">The destination scene the player should be warped to</param>
		/// <param name="returnScene">The scene the player should return to when leaving the destination scene</param>
		/// <param name="gateName">The name of the <see cref="TransitionPoint"/> the player should spawn at when entering the <paramref name="destinationScene"/></param>
		/// <param name="warpDelay">The delay before the player is transported to the new scene</param>
		/// <param name="noCharms">Should the player have no access to charms in the destination scene?</param>
		/// <param name="options">The customization options for the warp sequence</param>
		public static void DoDreamnailWarp(Vector3 position, string destinationScene, string returnScene,string gateName = "door1", float warpDelay = 1.75f, bool noCharms = false, DreamWarpOptions options = default)
		{
			UnboundCoroutine.Start(DoDreamnailWarpRoutine(position,destinationScene,returnScene,gateName,warpDelay,noCharms,options));
		}

		static IEnumerator DoDreamnailWarpRoutine(Vector3 effectsPosition, string destinationScene, string returnScene, string gateName, float warpDelay, bool noCharms, DreamWarpOptions options)
		{
			Warping = true;
			if (options == default)
			{
				options = DreamWarpDefaults;
			}
			PlayerData.instance.SetBool("disablePause", true);
			HeroController.instance.RelinquishControl();
			PlayMakerUtilities.SetFsmBool(HeroController.instance.gameObject, "Dream Nail", "Dream Convo", true);
			PlayMakerUtilities.SetFsmBool(HeroController.instance.gameObject, "ProxyFSM", "No Charms", noCharms);

			if (options.DreamWarpSound != null)
			{
				WeaverAudio.PlayAtPoint(options.DreamWarpSound, effectsPosition, channel: Enums.AudioChannel.Sound);
			}

			if (options.DreamParticles != null)
			{
				GameObject.Instantiate(options.DreamParticles, effectsPosition, Quaternion.identity);
			}

			PlayerData.instance.SetString("dreamReturnScene", returnScene);

			CameraShaker.Instance.Shake(Enums.ShakeType.AverageShake);


			var blanker = WeaverCanvas.HUDBlankerWhite;
			if (blanker != null)
			{
				PlayMakerUtilities.SetFsmFloat(blanker, "Blanker Control", "Fade Time", Mathf.Clamp(warpDelay - 0.25f,0f,100f));
				EventManager.SendEventToGameObject("FADE IN",blanker);
			}
			yield return null;

			if (!Application.isPlaying)
			{
				yield break;
			}

			EventManager.BroadcastEvent("BOX DOWN DREAM",null);

			yield return new WaitForSeconds(warpDelay);

			if (!Application.isPlaying)
			{
				yield break;
			}

			GameManager.instance.TimePasses();
			GameManager.instance.ResetSemiPersistentItems();
			PlayMakerUtilities.SetFsmBool(WeaverCamera.Instance.gameObject, "CameraFade", "No Fade", true);
			HeroController.instance.EnterWithoutInput(true);
			HeroController.instance.AcceptInput();
			GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
			{
				SceneName = destinationScene,
				EntryGateName = gateName,
				EntryDelay = 0f,
				Visualization = GameManager.SceneLoadVisualizations.Dream,
				PreventCameraFadeOut = true,
				WaitForSceneTransitionCameraFade = false,
				AlwaysUnloadUnusedAssets = false
			});

			//DREAM ENTRY EFFECTS

			bool inPosition = false;

            if (!HeroController.instance.isHeroInPosition)
            {
				HeroController.instance.heroInPosition += Instance_heroInPosition;

				void Instance_heroInPosition(bool forceDirect)
				{
					inPosition = true;
					HeroController.instance.heroInPosition -= Instance_heroInPosition;
				}

				yield return new WaitUntil(() => inPosition);
			}

			yield return null;

			HeroController.instance.RelinquishControl();
			HeroController.instance.StopAnimationControl();
			HeroController.instance.GetComponent<Renderer>().enabled = false;
			HeroController.instance.MaxHealth();
			EventManager.BroadcastEvent("UPDATE BLUE HEALTH",null);


			var door = GameObject.FindObjectsOfType<TransitionPoint>().FirstOrDefault(t => t.gameObject.name == gateName);

			if (door == null)
			{
				throw new Exception($"Unable to find the TransitionPoint gate of name {gateName} in scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
			}


			if (door.alwaysEnterRight)
			{
				HeroController.instance.FaceRight();
			}

			if (door.alwaysEnterLeft)
			{
				HeroController.instance.FaceLeft();
			}

			GameManager.instance.cameraCtrl.PositionToHero(forceDirect: false);
			PlayMakerUtilities.SetFsmFloat(blanker, "Blanker Control", "Fade Time", 1f);
			EventManager.SendEventToGameObject("FADE OUT", blanker);
			yield return new WaitForSeconds(0.75f);

			if (options.DreamArriveSound != null)
			{
				WeaverAudio.PlayAtPoint(options.DreamArriveSound, HeroController.instance.transform.position, channel: Enums.AudioChannel.Sound);
			}

			CameraShaker.Instance.Shake(Enums.ShakeType.AverageShake);

			var heroPos = HeroController.instance.transform.position;
			var heroScale = HeroController.instance.transform.localScale.x;

			GameObject effects = null;

			if (options.DreamArriveParticles != null)
			{
				effects = GameObject.Instantiate(options.DreamArriveParticles, heroPos, Quaternion.identity);
				effects.transform.SetScaleX(heroScale);
			}

			if (effects != null)
			{
				var effectsAnim = effects.GetComponent<WeaverAnimationPlayer>();
				if (effectsAnim != null)
				{
					yield return effectsAnim.WaitforClipToFinish();
				}
			}


			HeroController.instance.RegainControl();
			HeroController.instance.StartAnimationControl();
			HeroController.instance.GetComponent<Renderer>().enabled = true;

			if (effects != null)
			{
				GameObject.Destroy(effects);
			}

			Warping = false;

			yield break;
		}
	}
}
