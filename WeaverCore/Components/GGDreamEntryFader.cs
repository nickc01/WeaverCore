using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// Contains the fade-in effects that are played when the player enters a Godhome bossfight
    /// </summary>
    [RequireComponent(typeof(EventRegister), typeof(EventListener))]
    public class GGDreamEntryFader : DreamEntryFader
    {
        EventRegister register;
        EventListener listener;

        bool exitRoarInterupt = false;

        bool finishedPlayingClip = false;

        protected override void Awake()
        {
            transform.position = default;
            register = GetComponent<EventRegister>();
            listener = GetComponent<EventListener>();
            register.OnReceivedEvent += Register_OnReceivedEvent;

            listener.ListenForEvent("KNIGHT EXITED ROAR", (source, dest) =>
            {
                exitRoarInterupt = true;
            });

            if (Initialization.Environment == RunningState.Game)
            {
                StartCoroutine(GGMainRoutine());
            }
        }

        private void Register_OnReceivedEvent()
        {
            exitRoarInterupt = true;
        }

        IEnumerator GGMainRoutine()
        {
            yield return WaitForHero();

            if (!string.IsNullOrEmpty(EntryDoor))
            {
                if (HeroController.instance.GetEntryGateName() != EntryDoor)
                {
                    yield break;
                }
            }

            HeroController.instance.RelinquishControl();
            HeroController.instance.StopAnimationControl();
            var playerRenderer = HeroController.instance.GetComponent<Renderer>();

            // Check if in pantheon
            if (BossSequenceController.IsInSequence)
            {
                if (BossSequenceController.BossIndex < 1)
                {
                    // FIRST
                    EventRegister.SendEvent("K HATCHLING END");
                    playerRenderer.enabled = false;
                }
                else
                {
                    // FINISHED
                    yield return StartKneeling();
                }
            }
            else
            {
                // STATUE
                EventRegister.SendEvent("K HATCHLING END");
                yield return StartKneeling();
            }

            var hudBlanker = WeaverCanvas.HUDBlankerWhite;
            if (hudBlanker != null)
            {
                PlayMakerUtilities.SetFsmFloat(hudBlanker.gameObject, "Blanker Control", "Fade Time", 0.5f);
                EventManager.SendEventToGameObject("FADE OUT", hudBlanker.gameObject, gameObject);
            }

            yield return new WaitForSeconds(0.75f);

            var cutscene = GetComponentInChildren<WeaverAnimationPlayer>(true);

            if (BossSequenceController.IsInSequence && BossSequenceController.BossIndex < 1)
            {
                // TRUE
                if (dreamEnterAudio != null)
                {
                    WeaverAudio.PlayAtPoint(dreamEnterAudio, Player.Player1.transform.position);
                }

                CameraShaker.Instance.Shake(ShakeType.AverageShake);

                cutscene.transform.position = Player.Player1.transform.position;
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
            else
            {
                // FALSE
                HeroUtilities.PauseCurrentAnimation(false);
                exitRoarInterupt = false;
                StartCoroutine(PlayPlayerClip("Collect Normal 3"));

                yield return new WaitUntil(() => finishedPlayingClip || exitRoarInterupt);
            }

            playerRenderer.enabled = true;
            cutscene.gameObject.SetActive(false);
            HeroController.instance.RegainControl();
            HeroController.instance.StartAnimationControl();
            HeroController.instance.AcceptInput();
            HeroController.instance.ResetQuakeDamage();
        }

        /// <summary>
        /// Coroutine to play a specific player clip.
        /// </summary>
        /// <param name="playerClip">Name of the player clip to play.</param>
        IEnumerator PlayPlayerClip(string playerClip)
        {
            finishedPlayingClip = false;
            yield return HeroUtilities.PlayPlayerClipTillDone(playerClip);
            finishedPlayingClip = true;
        }

        /// <summary>
        /// Coroutine to start the kneeling animation.
        /// </summary>
        IEnumerator StartKneeling()
        {
            HeroUtilities.PlayPlayerClip("Collect Normal 3");
            HeroUtilities.PauseCurrentAnimation(true);

            yield return new WaitUntil(() => BossSceneController.Instance != null && BossSceneController.Instance.HasTransitionedIn);
        }
    }
}