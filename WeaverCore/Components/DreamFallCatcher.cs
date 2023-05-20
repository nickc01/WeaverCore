using System.Collections;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{


    /// <summary>
    /// When the player touches an object with this component attached, and they are in a dream scene, the player will get transported to the scene there were previously
    /// </summary>
    /// <remarks>Must have a collider attached to work</remarks>
    public class DreamFallCatcher : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("The sound effect that is played when the player touches this catcher")]
		AudioClip SoundEffect;

		[SerializeField]
		[Tooltip("The TransitionPoint the player will be placed at when returning")]
		string returningGateName = "door_dreamReturn";

		bool collidingWithPlayer = false;

		private void Start()
		{
			StartCoroutine(InitRoutine());
		}

		private void Reset()
		{
			SoundEffect = WeaverAssets.LoadWeaverAsset<AudioClip>("Dream Enter");
		}

		IEnumerator InitRoutine()
		{
			yield return WaitForHeroInPosition();

			yield return new WaitUntil(() => collidingWithPlayer);

			EventManager.SendEventToGameObject("FSM CANCEL", HeroController.instance.gameObject, gameObject);
			PlayMakerUtilities.SetFsmBool(HeroController.instance.gameObject, "ProxyFSM", "No Charms", false);
			HeroController.instance.RelinquishControl();
			if (SoundEffect != null)
			{
				WeaverAudio.PlayAtPoint(SoundEffect, transform.position, 1f, Enums.AudioChannel.Sound);
			}
			HeroController.instance.StopAnimationControl();

			var hudBlanker = WeaverCanvas.HUDBlankerWhite;
			if (hudBlanker != null)
			{
				PlayMakerUtilities.SetFsmFloat(hudBlanker.gameObject, "Blanker Control", "Fade Time", 0.9f);
				EventManager.SendEventToGameObject("FADE IN", hudBlanker.gameObject, gameObject);
			}

			yield return new WaitForSeconds(1f);

			GameManager.instance.TimePasses();
			GameManager.instance.ResetSemiPersistentItems();
			PlayMakerUtilities.SetFsmBool(WeaverCamera.Instance.gameObject, "CameraFade", "No Fade", true);
			PlayMakerUtilities.SetFsmBool(HeroController.instance.gameObject, "Dream Return", "Dream Returning", true);
			HeroController.instance.EnterWithoutInput(true);

			var returnScene = PlayerData.instance.GetString("dreamReturnScene");

			GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
			{
				SceneName = returnScene,
				EntryGateName = returningGateName,
				EntryDelay = 0f,
				Visualization = GameManager.SceneLoadVisualizations.Default,
				PreventCameraFadeOut = true,
				WaitForSceneTransitionCameraFade = false,
				AlwaysUnloadUnusedAssets = false
			});


			yield break;
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.CompareTag("Player"))
			{
                collidingWithPlayer = true;
            }
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
            if (collision.CompareTag("Player"))
            {
                collidingWithPlayer = false;
            }
        }


		IEnumerator WaitForHeroInPosition()
		{
			if (HeroController.instance != null && !HeroController.instance.isHeroInPosition)
			{
				bool inPosition = false;
				HeroController.HeroInPosition temp = null;
				temp = (bool forceDirect) =>
				{
					inPosition = true;
					HeroController.instance.heroInPosition -= temp;
				};
				HeroController.instance.heroInPosition += temp;
				yield return new WaitUntil(() => inPosition);
			}
		}
	}
}
