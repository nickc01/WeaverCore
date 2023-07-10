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

    public class GGStatueInspect : InspectRegion
    {
        [SerializeField]
        AudioClip challengeSound;

        [SerializeField]
        float challengeSoundDelay = 0.25f;

        private static GameObject spawnedUI;

        [SerializeField]
        CameraLockArea camLock;

        bool damaged = false;

        bool transitionEventHooked = false;

        bool fullyTransitionedOut = false;

        EventRegister eventRegister;

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

            var prefab = GG_Internal.bossUIPrefab;

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

                if (GG_Internal.dreamAreaEffect != null)
                {
                    GameObject.Instantiate(GG_Internal.dreamAreaEffect, Vector3.zero, Quaternion.identity);
                }

                foreach (Transform item in transform)
                {
                    item.gameObject.SetActive(true);
                }

                EventManager.BroadcastEvent("BOX DOWN DREAM", gameObject);

                EventManager.BroadcastEvent("CONVO CANCEL", gameObject);

                if (GG_Internal.ggBattleTransitions != null)
                {
                    var battleTransitions = GameObject.Find("gg_battle_transitions(Clone)");

                    if (battleTransitions == null)
                    {
                        battleTransitions = GameObject.Instantiate(GG_Internal.ggBattleTransitions, Vector3.zero, Quaternion.identity);
                    }

                    //var battleTransitions = GameObject.Instantiate(GG_Internal.ggBattleTransitions, Vector3.zero, Quaternion.identity);

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

    /*public class GGStatueInspect : MonoBehaviour
    {
        public bool Talking { get; private set; } = false;
        bool canTalk = true;
        bool facingRight = false;

        Transform PromptMarker;

        private void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("Hero Detector");
            PromptMarker = transform.Find("Prompt Marker");
            StartCoroutine(InspectRoutine());
        }


        IEnumerator InspectRoutine()
        {
            if (PromptMarker.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            {
                spriteRenderer.enabled = false;
            }

            var triggerTracker = gameObject.GetComponent<TrackTriggerObjects>();

            if (triggerTracker == null)
            {
                triggerTracker = gameObject.AddComponent<TrackTriggerObjects>();
            }

            yield return new WaitForSeconds(1f);

            while (true)
            {
                var prompt = WeaverArrowPrompt.Spawn(gameObject, PromptMarker.position);
                prompt.SetLabelTextLang("LISTEN");
                prompt.HideInstant();

                yield return new WaitUntil(() => triggerTracker.InsideCount > 0);

                if (canTalk && HeroController.instance.CanTalk())
                {
                    prompt.Show();

                    while (true)
                    {
                        if (triggerTracker.InsideCount == 0)
                        {
                            break;
                        }
                        else if (HeroController.instance.CanInspect() && (PlayerInput.down.WasPressed || PlayerInput.up.WasPressed))
                        {
                            Talking = true;
                            prompt.Hide();
                            yield return TalkRoutine();
                            Talking = false;
                        }
                        yield return null;
                    }
                }

                yield return null;
            }
        }

        IEnumerator TalkRoutine()
        {
            PlayerData.instance.SetBool("disablePause", true);

            HeroController.instance.RelinquishControl();
            HeroController.instance.StopAnimationControl();

            var selfX = transform.position.x;
            var playerX = Player.Player1.transform.position.x;

            float leftProx = selfX - 1.5f;
            float rightProx = selfX + 1.5f;

            bool forceTurnLeft = false;
            bool forceTurnRight = false;

            var heroRB = HeroController.instance.GetComponent<Rigidbody2D>();

            if (playerX >= selfX && playerX < rightProx)
            {
                //MOVE RIGHT
                Player.Player1.transform.SetScaleX(-1f);
                HeroController.instance.FaceRight();
                HeroUtilities.PlayPlayerClip("Walk");
                heroRB.velocity = new Vector2(6f, 0f);
                for (float t = 0; t < 1; t += Time.deltaTime)
                {
                    if (Player.Player1.transform.position.x >= rightProx)
                    {
                        break;
                    }
                    yield return null;
                }
            }
            else if (playerX >= leftProx && playerX < selfX)
            {
                //MOVE LEFT
                Player.Player1.transform.SetScaleX(1f);
                HeroController.instance.FaceLeft();
                HeroUtilities.PlayPlayerClip("Walk");
                heroRB.velocity = new Vector2(-6f, 0f);
                for (float t = 0; t < 1; t += Time.deltaTime)
                {
                    if (Player.Player1.transform.position.x <= leftProx)
                    {
                        break;
                    }
                    yield return null;
                }
            }

            var playerScaleX = Player.Player1.transform.localScale.x;

            var playerFacingRight = playerScaleX < 0;
            var playerOnRight = playerX >= selfX;

            if (forceTurnLeft || (playerFacingRight && playerOnRight))
            {
                //TURN HERO LEFT
                heroRB.velocity = default;
                HeroController.instance.FaceLeft();
                Player.Player1.transform.SetScaleX(1f);
                yield return HeroUtilities.PlayPlayerClipTillDone("Turn");
            }
            else if (forceTurnRight || (!playerFacingRight && !playerOnRight))
            {
                //TURN HERO RIGHT
                heroRB.velocity = default;
                HeroController.instance.FaceRight();
                Player.Player1.transform.SetScaleX(-1f);
                yield return HeroUtilities.PlayPlayerClipTillDone("Turn");
            }

            HeroUtilities.PlayPlayerClip("Idle");

            EventManager.BroadcastEvent("NPC CONVO START",gameObject);
        }
    }*/
}
