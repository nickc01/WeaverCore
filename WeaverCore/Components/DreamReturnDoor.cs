using System;
using System.Collections;
using UnityEngine;
using WeaverCore.Assets.Components;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    public class DreamReturnDoor : MonoBehaviour
	{
        [NonSerialized]
        DoorControl doorControl;

        [SerializeField]
        bool entering = false;

        [SerializeField]
        AudioClip dreamEnterSound;

        [SerializeField]
        string returningScene = "";

        [SerializeField]
        string returningGateName = "door_dreamReturn";


        private void Awake()
        {
            doorControl = GetComponent<DoorControl>();
            if (doorControl == null)
            {
                doorControl = gameObject.AddComponent<DoorControl>();
            }

            doorControl.PromptLabel = "LANGKEY:EXIT";
            //Debug.LogError("ADDING HOOK");
            doorControl.OnEnter.AddListener((knight_anim) =>
            {
                //Debug.LogError("STARTING ENTER 1");
                StartCoroutine(OnEnterRoutine());
            });


            //StartCoroutine(MainRoutine());
        }


        IEnumerator OnEnterRoutine()
        {
            if (entering)
            {
                yield break;
            }

            entering = true;

            HeroController.instance.RelinquishControl();
            HeroController.instance.StopAnimationControl();
            HeroUtilities.PlayPlayerClip("Collect Normal 1");
            PlayMakerUtilities.SetFsmBool(HeroController.instance.gameObject, "Dream Return", "Dream Returning", true);
            HeroController.instance.EnterWithoutInput(true);
            PlayerData.instance.SetBool("disablePause", true);

            yield return new WaitForSeconds(0.8f);

            if (dreamEnterSound != null)
            {
                WeaverAudio.PlayAtPoint(dreamEnterSound, Player.Player1.transform.position);
            }

            var particles = GetComponentInChildren<ParticleSystem>();
            var animation = GetComponentInChildren<WeaverAnimationPlayer>(true);

            animation.gameObject.SetActive(true);

            animation.transform.position = Player.Player1.transform.position + new Vector3(0,-1,-2);

            HeroController.instance.GetComponent<Renderer>().enabled = false;

            CameraShaker.Instance.Shake(Enums.ShakeType.AverageShake);

            particles.Play();

            yield return new WaitForSeconds(2f);

            var hudBlanker = WeaverCanvas.HUDBlankerWhite;
            if (hudBlanker != null)
            {
                PlayMakerUtilities.SetFsmFloat(hudBlanker.gameObject, "Blanker Control", "Fade Time", 0.9f);
                EventManager.SendEventToGameObject("FADE IN", hudBlanker.gameObject, gameObject);
            }

            yield return new WaitForSeconds(1f);

            HeroController.instance.GetComponent<Renderer>().enabled = true;

            var returnScene = PlayerData.instance.GetString("dreamReturnScene");

            if (!string.IsNullOrEmpty(returningScene))
            {
                returnScene = returningScene;
            }

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
        }

        /*IEnumerator MainRoutine()
        {
            var particles = GetComponentInChildren<ParticleSystem>();



            yield break;
        }*/
    }
}
