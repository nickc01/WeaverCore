using System;
using System.Collections;
using TMPro;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    public class DialogueInspectRegion : InspectRegion
    {
        public enum TextCentering
        {
            Center,
            CenterTop
        }

        [field: SerializeField]
        public TextCentering TextAlignment { get; private set; } = TextCentering.Center;

        bool damaged = false;

        [field: SerializeField]
        public string TextConvo { get; set; }

        [field: SerializeField]
        public string TextSheet { get; set; } = "CP3";

        bool dialogActive = false;

        public bool Talking => dialogActive;


        protected override void Awake()
        {
            base.Awake();
            OnKnightDamaged += DialogueInspectRegion_OnKnightDamaged;
            EventManager.OnEventTriggered += EventManager_OnEventTriggered;
        }

        protected override void OnDisable()
        {
            EventManager.OnEventTriggered -= EventManager_OnEventTriggered;
        }

        protected override void OnDestroy()
        {
            EventManager.OnEventTriggered -= EventManager_OnEventTriggered;
        }

        private int DialogueInspectRegion_OnKnightDamaged(int hazardType, int damageAmount)
        {
            damaged = true;
            return damageAmount;
        }

        protected override IEnumerator OnInspectRoutine()
        {
            EventManager.BroadcastEvent("NPC CONVO START", gameObject);

            damaged = false;

            var dialogueManager = GameObject.Find("DialogueManager");

            if (dialogueManager == null)
            {
                if (Initialization.Environment == Enums.RunningState.Game)
                {
                    WeaverLog.LogError("Couldn't find dialog manager");
                }
                else
                {
                    yield break;
                }
            }

            yield return CoroutineUtilities.RunWhile(MainRoutine(dialogueManager), () => !damaged);

            if (damaged)
            {
                EventManager.BroadcastEvent("NPC TITLE DOWN", gameObject);
                EventManager.SendEventToGameObject("BOX DOWN", dialogueManager, gameObject);
            }
        }

        IEnumerator MainRoutine(GameObject dialogueManager)
        {
            var text = dialogueManager.transform.Find("Text").GetComponent<TextMeshPro>();

            if (TextAlignment == TextCentering.Center)
            {
                text.alignment = TextAlignmentOptions.Center;
            }
            else if (TextAlignment == TextCentering.CenterTop)
            {
                text.alignment = TextAlignmentOptions.Top;
            }

            EventManager.SendEventToGameObject("BOX UP", dialogueManager, gameObject);

            yield return new WaitForSeconds(0.3f);

            var dialogueBox = text.GetComponent("DialogueBox");

            dialogueBox.ReflectCallMethod("StartConversation", new object[] { TextConvo, TextSheet });
            dialogActive = true;

            yield return new WaitUntil(() => !dialogActive);

            if (heroLooksUp)
            {
                HeroUtilities.PlayPlayerClip("LookUpToIdle");
            }

            EventManager.SendEventToGameObject("BOX DOWN", dialogueManager, gameObject);

            yield return new WaitForSeconds(0.3f);

            text.alignment = TextAlignmentOptions.TopLeft;
        }

        private void EventManager_OnEventTriggered(string eventName, GameObject source, GameObject destination, EventManager.EventType eventType)
        {
            if (dialogActive && eventType == EventManager.EventType.Broadcast && eventName == "CONVO_FINISH")
            {
                dialogActive = false;
            }
        }
    }
}
