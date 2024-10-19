using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Assets.Components;
using WeaverCore.Internal;
using WeaverCore.Utilities;

namespace WeaverCore.Components.GGStatue
{
    /// <summary>
    /// Represents a GG Statue inspect region.
    /// </summary>
    public class GGStatueInspect : InspectRegion
    {
        [SerializeField]
        [Tooltip("Sound played when the challenge starts.")]
        private AudioClip challengeSound;

        [SerializeField]
        [Tooltip("Delay before playing the challenge sound.")]
        private float challengeSoundDelay = 0.25f;

        private static GameObject spawnedUI;

        [SerializeField]
        [Tooltip("Camera lock area associated with the GG Statue.")]
        private CameraLockArea camLock;

        private bool damaged = false;

        private bool transitionEventHooked = false;

        private bool fullyTransitionedOut = false;

        private EventRegister eventRegister;

        /// <summary>
        /// Called when the object is awakened.
        /// </summary>
        protected override void Awake()
        {
            eventRegister = GetComponent<EventRegister>();
            OnKnightDamaged += GGStatueInspect_OnKnightDamaged;
            base.Awake();
        }

        private int GGStatueInspect_OnKnightDamaged(int hazardType, int damageAmount)
        {
            damaged = true;
            return damageAmount;
        }

        protected override IEnumerator OnInspectRoutine()
        {
            damaged = false;
            yield return CoroutineUtilities.RunWhile(MainConvoRoutine(), () => !damaged);

            camLock.gameObject.SetActive(false);
        }

        protected override void OnDestroy()
        {
            if (transitionEventHooked)
            {
                EventManager.OnEventTriggered -= EventManager_OnEventTriggered;
                transitionEventHooked = false;
            }
            base.OnDestroy();
        }

        protected override void OnDisable()
        {
            if (transitionEventHooked)
            {
                EventManager.OnEventTriggered -= EventManager_OnEventTriggered;
                transitionEventHooked = false;
            }
            base.OnDisable();
        }

        IEnumerator PlaySoundRoutine(AudioClip sound, float delay)
        {
            yield return new WaitForSeconds(delay);
            WeaverAudio.PlayAtPoint(sound, Player.Player1.transform.position);
        }

        IEnumerator MainConvoRoutine()
        {
            EventManager.BroadcastEvent("NPC CONVO START",gameObject);

            HeroUtilities.PlayPlayerClip("LookUp");
            camLock.gameObject.SetActive(true);

            var prefab = GG_Preloads.bossUIPrefab;

            if (prefab == null)
            {
                if (Initialization.Environment == Enums.RunningState.Editor)
                {
                    Debug.LogError("Cannot use the GG Statue in the Unity Editor. Launch the game first in order to test it out");
                }
                else
                {
                    throw new System.Exception("Unable to find boss UI Prefab");
                }
            }

            if (spawnedUI == null && prefab != null)
            {
                spawnedUI = GameObject.Instantiate(prefab);
                spawnedUI.SetActive(value: false);
            }

            bool done = false;

            bool selected = false;

            spawnedUI.transform.position = prefab.transform.position;
            spawnedUI.SetActive(value: true);
            BossChallengeUI ui = spawnedUI.GetComponent<BossChallengeUI>();
            if (ui != null)
            {
                BossStatue statue = GetComponentInParent<BossStatue>();
                BossChallengeUI.HideEvent temp2 = null;
                temp2 = delegate
                {
                    done = true;
                    ui.OnCancel -= temp2;
                };
                ui.OnCancel += temp2;
                BossChallengeUI.LevelSelectedEvent temp = null;
                temp = delegate
                {
                    //TODO - LEVEL SELECTED STATE
                    //base.Fsm.Event(levelSelectedEvent);
                    selected = true;
                    done = true;
                    ui.OnLevelSelected -= temp;
                };
                ui.OnLevelSelected += temp;

                var details = statue.UsingDreamVersion ? statue.dreamBossDetails : statue.bossDetails;

                //WeaverLog.Log("DETAILS = " + JsonUtility.ToJson(details, true));

                ui.Setup(statue, details.nameSheet, details.nameKey, details.descriptionSheet, details.descriptionKey);
            }



            yield return new WaitUntil(() => done);

            if (selected)
            {
                PlayerData.instance.SetBool("disablePause", true);
                HeroController.instance.RelinquishControl();
                PlayMakerUtilities.SetFsmBool(HeroController.instance.gameObject, "Dream Nail", "Dream Convo", true);
                GetComponent<Collider2D>().enabled = false;

                var heroX = Player.Player1.transform.position.x;
                var selfX = transform.position.x;
                if (heroX <= selfX)
                {
                    //HERO ON LEFT
                    HeroController.instance.FaceLeft();
                }
                else
                {
                    //HERO ON RIGHT
                    HeroController.instance.FaceRight();
                }

                if (challengeSound != null)
                {
                    //WeaverAudio.PlayAtPoint(challengeSound, Player.Player1.transform.position);
                    StartCoroutine(PlaySoundRoutine(challengeSound, challengeSoundDelay));
                }

                yield return HeroUtilities.PlayPlayerClipTillDone("Challenge Start");

                //DREAM ENTER EVENT

                //AUDIO PLAY SIMPLE??

                PlayerData.instance.SetString("dreamReturnScene", "GG_Workshop");

                if (GG_Preloads.dreamAreaEffect != null)
                {
                    GameObject.Instantiate(GG_Preloads.dreamAreaEffect, Vector3.zero, Quaternion.identity);
                }

                foreach (Transform item in transform)
                {
                    item.gameObject.SetActive(true);
                }

                EventManager.BroadcastEvent("BOX DOWN DREAM", gameObject);

                EventManager.BroadcastEvent("CONVO CANCEL", gameObject);

                if (GG_Preloads.ggBattleTransitions != null)
                {
                    var battleTransitions = GameObject.Find("gg_battle_transitions(Clone)");

                    if (battleTransitions == null)
                    {
                        battleTransitions = GameObject.Instantiate(GG_Preloads.ggBattleTransitions, Vector3.zero, Quaternion.identity);
                    }

                    //var battleTransitions = GameObject.Instantiate(GG_Preloads.ggBattleTransitions, Vector3.zero, Quaternion.identity);

                    EventRegister.SendEvent("GG TRANSITION OUT");
                    /*EventManager.SendEventToGameObject("GG TRANSITION OUT", battleTransitions, gameObject);*/

                    transitionEventHooked = true;
                    fullyTransitionedOut = false;
                    eventRegister.OnReceivedEvent += EventRegister_OnReceivedEvent;
                    EventManager.OnEventTriggered += EventManager_OnEventTriggered;

                    yield return new WaitUntil(() => fullyTransitionedOut);
                    HeroController.instance.ClearMPSendEvents();

                    GameManager.instance.TimePasses();
                    GameManager.instance.ResetSemiPersistentItems();

                    PlayMakerUtilities.SetFsmBool(WeaverCamera.Instance.gameObject, "CameraFade", "No Fade", true);
                    HeroController.instance.EnterWithoutInput(true);
                    HeroController.instance.AcceptInput();

                    var bossSceneToLoad = StaticVariableList.GetValue<string>("bossSceneToLoad");


                    GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
                    {
                        SceneName = bossSceneToLoad,
                        EntryGateName = "door_dreamEnter",
                        EntryDelay = 0f,
                        Visualization = GameManager.SceneLoadVisualizations.GodsAndGlory,
                        PreventCameraFadeOut = true,
                        WaitForSceneTransitionCameraFade = false,
                        AlwaysUnloadUnusedAssets = false
                    });

                }

            }
            else
            {
                spawnedUI.SetActive(value: false);
            }

            yield break;
        }

        private void EventRegister_OnReceivedEvent()
        {
            if (transitionEventHooked)
            {
                EventManager.OnEventTriggered -= EventManager_OnEventTriggered;
                transitionEventHooked = false;
                fullyTransitionedOut = true;
            }
        }

        private void EventManager_OnEventTriggered(string eventName, GameObject source, GameObject destination, EventManager.EventType eventType)
        {
            if (eventName == "GG TRANSITION END")
            {
                EventManager.OnEventTriggered -= EventManager_OnEventTriggered;
                transitionEventHooked = false;
                fullyTransitionedOut = true;
            }
        }
    }
}
