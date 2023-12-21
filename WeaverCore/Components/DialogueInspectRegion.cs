using System;
using System.Collections;
using TMPro;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// Represents a inspection region that triggers dialogue.
    /// </summary>
    public class DialogueInspectRegion : InspectRegion
    {
        /// <summary>
        /// Enumeration for text centering options.
        /// </summary>
        public enum TextCentering
        {
            Center,
            CenterTop
        }

        /// <summary>
        /// Gets or sets the text alignment.
        /// </summary>
        [field: SerializeField]
        public TextCentering TextAlignment { get; private set; } = TextCentering.Center;

        private bool damaged = false;

        /// <summary>
        /// Gets or sets the conversation text key.
        /// </summary>
        [field: SerializeField]
        public string TextConvo { get; set; }

        /// <summary>
        /// Gets or sets the text sheet.
        /// </summary>
        [field: SerializeField]
        public string TextSheet { get; set; } = "CP3";

        private bool dialogActive = false;

        /// <summary>
        /// Gets a value indicating whether the dialogue is currently active.
        /// </summary>
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

        /// <summary>
        /// Handles the knight being damaged during dialogue.
        /// </summary>
        /// <param name="hazardType">The type of hazard.</param>
        /// <param name="damageAmount">The amount of damage.</param>
        /// <returns>The adjusted damage amount.</returns>
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

        private IEnumerator MainRoutine(GameObject dialogueManager)
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

        /// <summary>
        /// Handles the event triggered by the EventManager.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="source">The source GameObject of the event.</param>
        /// <param name="destination">The destination GameObject of the event.</param>
        /// <param name="eventType">The type of event.</param>
        private void EventManager_OnEventTriggered(string eventName, GameObject source, GameObject destination, EventManager.EventType eventType)
        {
            if (dialogActive && eventType == EventManager.EventType.Broadcast && eventName == "CONVO_FINISH")
            {
                dialogActive = false;
            }
        }
    }
}