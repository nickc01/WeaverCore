using System.Collections;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{

    public class DreamEntryFader : MonoBehaviour
	{
        [SerializeField]
        [Tooltip("Specifies which specific door in the scene should the player come from for this entry fader to activate. Leave blank for any entry door")]
        protected string entryDoor = "";

        [SerializeField]
        protected bool heroFacesLeft = false;

        [SerializeField]
        protected AudioClip dreamEnterAudio;


        public string EntryDoor => entryDoor;


        protected virtual void Awake()
        {
            StartCoroutine(MainRoutine());
        }

        IEnumerator MainRoutine()
        {
            yield return WaitForHero();

            if (!string.IsNullOrEmpty(EntryDoor))
            {
                if (HeroController.instance.GetEntryGateName() != EntryDoor)
                {
                    yield break;
                }
            }

            //WeaverLog.Log("DREAM ENTRY START");
            HeroController.instance.RelinquishControl();
            HeroController.instance.StopAnimationControl();
            var playerRenderer = HeroController.instance.GetComponent<Renderer>();
            playerRenderer.enabled = false;
            HeroController.instance.MaxHealth();
            EventManager.BroadcastEvent("UPDATE BLUE HEALTH", gameObject);

            yield return null;

            playerRenderer.enabled = false;

            if (heroFacesLeft)
            {
                HeroController.instance.FaceLeft();
            }
            else
            {
                HeroController.instance.FaceRight();
            }

            GameManager.instance.cameraCtrl.PositionToHero(false);
            var hudBlanker = WeaverCanvas.HUDBlankerWhite;
            if (hudBlanker != null)
            {
                PlayMakerUtilities.SetFsmFloat(hudBlanker.gameObject, "Blanker Control", "Fade Time", 1f);
                EventManager.SendEventToGameObject("FADE OUT", hudBlanker.gameObject, gameObject);
            }
            playerRenderer.enabled = false;
            yield return new WaitForSeconds(0.75f);
            playerRenderer.enabled = false;

            if (dreamEnterAudio != null)
            {
                WeaverAudio.PlayAtPoint(dreamEnterAudio, Player.Player1.transform.position);
            }

            var cutscene = GetComponentInChildren<WeaverAnimationPlayer>(true);

            CameraShaker.Instance.Shake(ShakeType.AverageShake);

            cutscene.transform.position = Player.Player1.transform.position;// + new Vector3(0, 0.78226f);
            cutscene.transform.localScale = Player.Player1.transform.localScale;

            cutscene.gameObject.SetActive(true);

            yield return null;

            yield return cutscene.WaitforClipToFinish();

            HeroController.instance.RegainControl();
            HeroController.instance.StartAnimationControl();
            HeroController.instance.AcceptInput();
            playerRenderer.enabled = true;
            cutscene.gameObject.SetActive(false);
        }

        protected IEnumerator WaitForHero()
        {
            if (HeroController.instance == null)
            {
                yield return new WaitUntil(() => HeroController.instance != null);
            }
            if (!HeroController.instance.isHeroInPosition)
            {
                bool inPosition = false;
                HeroController.HeroInPosition func = null;
                func = (forceDirect) =>
                {
                    inPosition = true;
                    HeroController.instance.heroInPosition -= func;

                };
                HeroController.instance.heroInPosition += func;

                yield return new WaitUntil(() => inPosition);
            }
        }
    }
}
