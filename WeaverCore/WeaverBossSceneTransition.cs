﻿using System.Collections;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
    /// <summary>
    /// Used for playing scene transitions in Godhome fights. This gets played when a boss is defeated
    /// </summary>
    public class WeaverBossSceneTransition : MonoBehaviour
    {
        EventManager listener;

        [SerializeField]
        [Tooltip("Dont touch this unless you know what you are doing")]
        bool playNextSceneEvents = true;

        private void Awake()
        {
            listener = GetComponent<EventManager>();
            if (listener == null)
            {
                listener = gameObject.AddComponent<EventManager>();
            }

            listener.AddReceiverForEvent("DREAM RETURN", BeginDreamReturn);

            listener.AddReceiverForEvent("DREAM EXIT", BeginDreamExit);
        }

        public void BeginDreamReturn()
        {
            StopAllCoroutines();
            StartCoroutine(DreamReturn());
            if (playNextSceneEvents)
            {
                StartCoroutine(NextSceneTransitions());
            }
        }

        public void BeginDreamExit()
        {
            StopAllCoroutines();
            StartCoroutine(DreamExit());
        }

        IEnumerator NextSceneTransitions()
        {
            var hudBlanker = WeaverCanvas.HUDBlankerWhite;
            if (hudBlanker != null)
            {
                PlayMakerUtilities.SetFsmFloat(hudBlanker.gameObject, "Blanker Control", "Fade Time", 0f);
                EventManager.SendEventToGameObject("FADE IN", hudBlanker.gameObject, gameObject);
            }
            yield return null;

            GameManager.instance.TimePasses();
            GameManager.instance.ResetSemiPersistentItems();

            PlayMakerUtilities.SetFsmBool(WeaverCamera.Instance.gameObject, "CameraFade", "No Fade", true);

            HeroController.instance.RelinquishControl();
            HeroController.instance.EnterWithoutInput(true);
        }

        IEnumerator DreamExit()
        {
            if (StaticVariableList.Exists("ggCinematicEnding") && StaticVariableList.GetValue<bool>("ggCinematicEnding"))
            {
                CinematicEnding();
            }
            else
            {
                if (StaticVariableList.Exists("ggEndScene"))
                {
                    yield return DreamFinish(StaticVariableList.GetValue<string>("ggEndScene"));
                }
            }
        }

        void CinematicEnding()
        {
            StaticVariableList.SetValue("ggCinematicEnding", false);

            string cinematicScene;
            if (PlayerData.instance.GetBool("givenGodseekerFlower"))
            {
                //Ending E
                cinematicScene = Constants.GetConstantValue<string>("ENDING_E_CINEMATIC");
            }
            else
            {
                //Ending D
                cinematicScene = Constants.GetConstantValue<string>("ENDING_D_CINEMATIC");
            }

            StaticVariableList.SetValue("skipEndWhiteFader", true);

            GameManager.instance.ChangeToScene(cinematicScene, "door1", 0f);
        }

        IEnumerator DreamReturn()
        {
            PlayMakerUtilities.SetFsmBool(Player.Player1.gameObject, "Dream Return", "Dream Returning", true);
            var dreamReturnScene = PlayerData.instance.GetString("dreamReturnScene");

            //EventManager.SendEventToGameObject("FADE OUT INSTANT", WeaverCamera.Instance.gameObject, gameObject);

            var hudBlanker = WeaverCanvas.HUDBlankerWhite;
            if (hudBlanker != null)
            {
                PlayMakerUtilities.SetFsmFloat(hudBlanker.gameObject, "Blanker Control", "Fade Time", 0f);
                EventManager.SendEventToGameObject("FADE IN", hudBlanker.gameObject, gameObject);
            }

            HeroController.instance.MaxHealth();

            yield return DreamFinish(dreamReturnScene);
        }

        IEnumerator DreamFinish(string returnScene)
        {
            PlayMakerUtilities.SetFsmBool(WeaverCamera.Instance.gameObject, "CameraFade", "No Fade", true);

            var bossReturnEntryGate = PlayerData.instance.GetString("bossReturnEntryGate");

            StaticVariableList.SetValue("finishedBossReturning", true);

            yield return null;

            var hudCamera = GameObject.FindObjectOfType<HUDCamera>()?.gameObject;
            if (hudCamera != null)
            {
                var hudCanvas = hudCamera.transform.Find("Hud Canvas")?.gameObject;

                if (hudCanvas != null)
                {
                    EventManager.SendEventToGameObject("OUT", hudCanvas, gameObject);
                }
            }

            HeroController.instance.RelinquishControl();
            HeroController.instance.StopAnimationControl();
            HeroController.instance.EnterWithoutInput(true);

            GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
            {
                SceneName = returnScene,
                EntryGateName = bossReturnEntryGate,
                EntryDelay = 0f,
                Visualization = GameManager.SceneLoadVisualizations.GodsAndGlory,
                PreventCameraFadeOut = true,
                WaitForSceneTransitionCameraFade = false,
                AlwaysUnloadUnusedAssets = false
            });
        }
    }
}
