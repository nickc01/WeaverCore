using System;
using System.Collections;
using UnityEngine;
using WeaverCore.Components;

namespace WeaverCore.Assets.Components
{
    /// <summary>
    /// Used to allow the player to interact with a door <see cref="TransitionPoint"/>
    /// </summary>
    public class DoorControl : MonoBehaviour
    {
        public bool PlayerInRange { get; private set; }

        bool overHero = false;

        [SerializeField]
        GameObject promptMarker;

        [SerializeField]
        [Tooltip("The delay before the player can use this doorway")]
        float activationDelay = 1f;

        WeaverArrowPrompt prompt;

        bool started = false;

        private void Awake()
        {
            started = true;
            StartCoroutine(StartRoutine());
        }

        private void OnEnable()
        {
            if (!started)
            {
                started = true;
                StartCoroutine(DoorRoutine());
            }
        }

        private void OnDisable()
        {
            started = false;
            StopAllCoroutines();
            if (prompt != null)
            {
                prompt.Hide();
                prompt = null;
            }
        }

        IEnumerator StartRoutine()
        {
            yield return WaitUntilInScene();
            yield return new WaitForSeconds(0.2f);

            if (promptMarker == null)
            {
                promptMarker = transform.Find("Prompt Marker")?.gameObject;
            }

            if (promptMarker == null)
            {
                promptMarker = new GameObject("Prompt Marker");

                promptMarker.transform.SetParent(transform);

                promptMarker.transform.localPosition = new Vector3(0f, 4f, 0f);
            }
            yield return new WaitForSeconds(1f);
            yield return DoorRoutine();
        }

        IEnumerator DoorRoutine()
        {
            while (true)
            {
                if (prompt != null)
                {
                    prompt.Hide();
                    prompt = null;
                }
                yield return new WaitUntil(() => PlayerInRange);

                prompt = WeaverArrowPrompt.Spawn(gameObject, promptMarker.transform.position);
                prompt.SetLabelTextLang("ENTER");
                prompt.Show();

                while (true)
                {
                    if ((PlayerInput.up.WasPressed || PlayerInput.down.WasPressed) && HeroController.instance.CanInteract())
                    {
                        yield return EnterRoutine();
                        yield break;
                    }
                    else if (!PlayerInRange)
                    {
                        break;
                    }
                    yield return null;
                }
            }
        }

        IEnumerator WaitUntilInScene()
        {
            if (!GameManager.instance.HasFinishedEnteringScene)
            {
                bool inScene = false;

                GameManager.EnterSceneEvent enteredScene = null;
                enteredScene = () =>
                {
                    GameManager.instance.OnFinishedEnteringScene -= enteredScene;
                    inScene = true;
                };
                GameManager.instance.OnFinishedEnteringScene += enteredScene;

                yield return new WaitUntil(() => inScene);
            }
        }

        IEnumerator EnterRoutine()
        {
            var doorWay = GetComponent<WeaverTransitionPoint>();
            if (doorWay != null)
            {
                yield return doorWay.DoTransition(overHero ? "Exit" : "Enter");
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            PlayerInRange = true;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            PlayerInRange = false;
        }
    }
}
