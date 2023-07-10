using System.Collections;
using System.Linq;
using UnityEngine;
using WeaverCore.Internal;

namespace WeaverCore.Components.GGStatue
{
    public class GGReturnFromBossVisualization : MonoBehaviour
    {
        [SerializeField]
        GameObject inspect;

        private void Awake()
        {
            if (HeroController.instance.isHeroInPosition)
            {
                StartCoroutine(VisualizationRoutine());
            }
            else
            {
                HeroController.instance.heroInPosition += Instance_heroInPosition;
            }
        }

        private void Instance_heroInPosition(bool forceDirect)
        {
            HeroController.instance.heroInPosition -= Instance_heroInPosition;
            StartCoroutine(VisualizationRoutine());
        }

        IEnumerator VisualizationRoutine()
        {
            var currentDoor = gameObject.name;

           // WeaverLog.Log("CURRENT DOOR NAME = " + currentDoor);

            var entryGate = HeroController.instance.GetEntryGateName();
            //WeaverLog.Log("ENTRY GATE = " + entryGate);
            if (currentDoor == entryGate)
            {
                PlayerData.instance.SetString("bossReturnEntryGate", "");
            }
            else
            {
                yield break;
            }

            inspect.SetActive(false);

            HeroController.instance.ClearMPSendEvents();

            yield return new WaitForSeconds(0.25f);

            GameObject transitions = null;

            if (GG_Internal.ggBattleTransitions != null)
            {
                //transitions = GameObject.Find("gg_battle_transitions(Clone)");
                /*transitions = GameObject.FindObjectsOfType<EventRegister>(true).Select(e => e.gameObject).FirstOrDefault(g => g.name.StartsWith("gg_battle_transitions"));*/
                //WeaverLog.Log("Found Transition Object = " + transitions);
                if (transitions == null)
                {
                    transitions = GameObject.Instantiate(GG_Internal.ggBattleTransitions);
                }
                //EventManager.SendEventToGameObject("GG TRANSITION OUT INSTANT", transitions, gameObject);
                EventRegister.SendEvent("GG TRANSITION OUT INSTANT");
            }

            var hudCamera = GameObject.FindObjectOfType<HUDCamera>()?.gameObject;
            if (hudCamera != null)
            {
                var hudCanvas = hudCamera.transform.Find("Hud Canvas")?.gameObject;

                if (hudCanvas != null)
                {
                    EventManager.SendEventToGameObject("IN", hudCanvas, gameObject);
                }
            }

            yield return new WaitForSeconds(0.5f);

            if (transitions != null)
            {
                //EventManager.SendEventToGameObject("GG TRANSITION IN STATUE", transitions, gameObject);
                EventRegister.SendEvent("GG TRANSITION IN STATUE");
            }

            yield return new WaitForSeconds(1f);

            inspect.SetActive(true);


            yield break;
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
