using System.Collections;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    public class ItemInspectRegion : InspectRegion
    {
        [SerializeField]
        [Tooltip("The sound that is played when the knight picks up the inspected item")]
        AudioClip itemPickupSound;

        [SerializeField]
        [Tooltip("If set to true, the knight will warp out of the dream scene when the item is fully inspected")]
        bool dreamExit;

        protected override void Awake()
        {
            OnKnightDamaged += OnKnightDamage;
            base.Awake();
        }

        protected override IEnumerator OnInspectRoutine()
        {
            GameObject target;

            if (transform.parent != null)
            {
                target = transform.parent.gameObject;
            }
            else
            {
                target = gameObject;
            }

            EventManager.SendEventToGameObject("STOP", target, gameObject);
            HeroUtilities.PlayPlayerClip("Collect Normal 1");
            yield return new WaitForSeconds(0.75f);

            if (target.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.velocity = default;
            }

            OnInspect?.Invoke();
            FullyInspected = true;

            HeroUtilities.PlayPlayerClip("Collect Normal 2");

            //TODO : Spawn Item Get Effect R at position Vector3(0, -0.76, -1) relative to the Player

            if (itemPickupSound != null)
            {
                WeaverAudio.PlayAtPoint(itemPickupSound, transform.position);
            }

            yield return new WaitForSeconds(1f);

            if (dreamExit)
            {
                EnableKnightDamageInterrupt = false;
                yield return DreamWarpRoutine();
            }
            else
            {
                yield return HeroUtilities.PlayPlayerClipTillDone("Collect Normal 3");
                OnFinishInspect();
            }

            yield break;
        }

        IEnumerator DreamWarpRoutine()
        {
            EventManager.BroadcastEvent("WHITE PALACE END", gameObject);
            yield return new WaitForSeconds(0.75f);
            HeroController.instance.RelinquishControl();
            HeroController.instance.StopAnimationControl();

            var hudBlanker = WeaverCanvas.HUDBlankerWhite;
            if (hudBlanker != null)
            {
                PlayMakerUtilities.SetFsmFloat(hudBlanker.gameObject, "Blanker Control", "Fade Time", 1.9f);
                EventManager.SendEventToGameObject("FADE IN", hudBlanker.gameObject, gameObject);
            }

            yield return new WaitForSeconds(2f);

            GameManager.instance.TimePasses();
            GameManager.instance.ResetSemiPersistentItems();
            PlayMakerUtilities.SetFsmBool(WeaverCamera.Instance.gameObject, "CameraFade", "No Fade", true);
            PlayMakerUtilities.SetFsmBool(HeroController.instance.gameObject, "Dream Return", "Dream Returning", true);
            HeroController.instance.EnterWithoutInput(true);

            var returnScene = PlayerData.instance.GetString("dreamReturnScene");

            string returningGateName;

            if (GameManager.instance.sm is WeaverSceneManager wsm)
            {
                returningGateName = wsm.DreamReturnGateName;
            }
            else
            {
                returningGateName = "door_dreamReturn";
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

            yield break;
        }



        int OnKnightDamage(int hazardType, int damageAmount)
        {
            if (EnableKnightDamageInterrupt && FullyInspected)
            {
                OnFinishInspect();
            }

            return damageAmount;
        }

        void OnFinishInspect()
        {
            GameManager.instance.CheckAllAchievements();
            Inspectable = false;
            EventManager.BroadcastEvent("SHINY PICKED UP", gameObject);
        }
    }
}
