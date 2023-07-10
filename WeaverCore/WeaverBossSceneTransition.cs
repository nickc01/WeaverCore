using System.Collections;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
    public class WeaverBossSceneTransition : MonoBehaviour
    {
        EventManager listener;

        //string destScene = "";
        //string entryDoor = "";

        private void Awake()
        {
            listener = GetComponent<EventManager>();
            if (listener == null)
            {
                listener = gameObject.AddComponent<EventManager>();
            }

            listener.AddReceiverForEvent("DREAM RETURN", BeginDreamReturn);

            listener.AddReceiverForEvent("DREAM EXIT", BeginDreamExit);
            /*listener.AddReceiverForEvent((e, source, dest) =>
            {
                if (e == "DREAM EXIT")
                {
                    WeaverLog.Log("DREAM EXIT EVENT");
                    StopAllCoroutines();
                    DreamExit();
                }
                else if (e == "DREAM RETURN")
                {
                    WeaverLog.Log("DREAM RETURN EVENT");
                    StopAllCoroutines();
                    DreamReturn();
                    //FadeOut();
                }
            });*/
        }

        public void BeginDreamReturn()
        {
            //WeaverLog.Log("DREAM RETURN EVENT");
            StopAllCoroutines();
            DreamReturn();
        }

        public void BeginDreamExit()
        {
            //WeaverLog.Log("DREAM EXIT EVENT");
            StopAllCoroutines();
            DreamExit();
        }

        /*void FadeOut()
        {
            var hudBlanker = WeaverCanvas.HUDBlankerWhite;
            if (hudBlanker != null)
            {
                PlayMakerUtilities.SetFsmFloat(hudBlanker.gameObject, "Blanker Control", "Fade Time", 0f);
                EventManager.SendEventToGameObject("FADE IN", hudBlanker.gameObject, gameObject);
            }

            GameManager.instance.TimePasses();
            GameManager.instance.ResetSemiPersistentItems();
            PlayMakerUtilities.SetFsmBool(WeaverCamera.Instance.gameObject, "CameraFade", "No Fade", true);
            HeroController.instance.RelinquishControl();
            HeroController.instance.EnterWithoutInput(true);

            GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
            {
                SceneName = destScene,
                EntryGateName = entryDoor,
                EntryDelay = 0f,
                Visualization = GameManager.SceneLoadVisualizations.GodsAndGlory,
                PreventCameraFadeOut = true,
                WaitForSceneTransitionCameraFade = false,
                AlwaysUnloadUnusedAssets = false
            });
        }*/

        void DreamExit()
        {
            if (StaticVariableList.Exists("ggCinematicEnding") && StaticVariableList.GetValue<bool>("ggCinematicEnding"))
            {
                CinematicEnding();
            }
            else
            {
                if (StaticVariableList.Exists("ggEndScene"))
                {
                    DreamFinish(StaticVariableList.GetValue<string>("ggEndScene"));
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

        void DreamReturn()
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

            DreamFinish(dreamReturnScene);

            /*yield return new WaitForSeconds(1f);

            GameManager.instance.TimePasses();
            GameManager.instance.ResetSemiPersistentItems();
            PlayMakerUtilities.SetFsmBool(WeaverCamera.Instance.gameObject, "CameraFade", "No Fade", true);
            PlayMakerUtilities.SetFsmBool(HeroController.instance.gameObject, "Dream Return", "Dream Returning", true);
            HeroController.instance.EnterWithoutInput(true);*/

        }

        void DreamFinish(string returnScene)
        {
            PlayMakerUtilities.SetFsmBool(WeaverCamera.Instance.gameObject, "CameraFade", "No Fade", true);

            var bossReturnEntryGate = PlayerData.instance.GetString("bossReturnEntryGate");

            StaticVariableList.SetValue("finishedBossReturning", true);

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
