using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
    /// <summary>
    /// The base class for all conversations with an NPC. You can setup a conversation by inheriting from this class, adding the component to an NPC object, and using <see cref="Speak(string)"/> and <see cref="PresentYesNoQuestion(string)"/> in <see cref="DoConversation"/> to talk with the player
    /// </summary>
    public abstract class Conversation : MonoBehaviour
    {
        /// <summary>
        /// The result of giving the player a yes/no question prompt
        /// </summary>
        public enum YesNoResult
        {
            /// <summary>
            /// Neither options were selected (shouldn't happen normally)
            /// </summary>
            Neither,
            /// <summary>
            /// The player selected "Yes"
            /// </summary>
            Yes,
            /// <summary>
            /// The player selected "No"
            /// </summary>
            No
        }

        bool talking = false;

        [field: SerializeField]
        public bool ShowBoxOnStart { get; set; } = true;

        [field: SerializeField]
        public float BeginConvoDelay { get; set; } = 0.3f;

        /// <summary>
        /// The result of the yes/no dialog box
        /// </summary>
        public YesNoResult DialogBoxResult { get; private set; }

        EventManager eventReceiver;

        bool boxVisible = false;

        [field: SerializeField]
        public bool UseFallbackCanvasIfNoDialogueManager { get; set; } = true;

        [field: SerializeField]
        public KeyCode FallbackAdvanceKey { get; set; } = KeyCode.Return;

        [field: SerializeField]
        public float FallbackInputDebounceTime { get; set; } = 0.1f;

        GameObject fallbackCanvasObj;
        GameObject fallbackPanelObj;
        Text fallbackMainText;
        Text fallbackPromptText;

        /// <summary>
        /// Override this method to customize the conversation with a player. Use <see cref="Speak(string)"/> to speak with the player, and use <see cref="PresentYesNoQuestion(string)"/> to prevent the player with a yes/no question
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerator DoConversation();

        /// <summary>
        /// Starts a conversation with the player
        /// </summary>
        public void StartConversation()
        {
            StartCoroutine(StartConversationRoutine());
        }

        public void StartConversation(bool makePlayerLookUp, Action onDone = null)
        {
            StartCoroutine(StartConversationRoutine(makePlayerLookUp, onDone));
        }

        private void EventReceiver_OnReceivedEvent(string eventName, GameObject source)
        {
            if (eventName == "YES")
            {
                DialogBoxResult = YesNoResult.Yes;
            }
            else if (eventName == "NO")
            {
                DialogBoxResult = YesNoResult.No;
            }
            else if (eventName == "CONVO_FINISH" && talking)
            {
                talking = false;
            }
        }

        /// <summary>
        /// Starts a conversation with the player
        /// </summary>
        /// <param name="makePlayerLookUp">Should the player look up when starting the convo?</param>
        public IEnumerator StartConversationRoutine(bool makePlayerLookUp = true, Action onDone = null)
        {
            if (eventReceiver == null)
            {
                if (!TryGetComponent(out eventReceiver))
                {
                    eventReceiver = gameObject.AddComponent<EventManager>();
                }
                eventReceiver.OnReceivedEvent += EventReceiver_OnReceivedEvent;
            }
            if (makePlayerLookUp)
            {
                HeroUtilities.PlayPlayerClip("LookUp");
            }

            /*var dialogManager = GameObject.Find("DialogueManager");
            if (dialogManager != null)
            {
                EventManager.SendEventToGameObject("BOX UP", dialogManager, gameObject);
            }*/
            boxVisible = false;
            if (ShowBoxOnStart)
            {
                ShowConversationBox();
            }
            yield return new WaitForSeconds(BeginConvoDelay);
            yield return DoConversation();
            DestroyFallbackCanvas();
            onDone?.Invoke();

        }

        /// <summary>
        /// Shows the conversation box
        /// </summary>
        public void ShowConversationBox()
        {
            if (!boxVisible)
            {
                boxVisible = true;
                var dialogManager = GameObject.Find("DialogueManager");
                if (dialogManager != null)
                {
                    EventManager.SendEventToGameObject("BOX UP", dialogManager, gameObject);
                }
                else if (UseFallbackCanvasIfNoDialogueManager)
                {
                    EnsureFallbackCanvas();
                    fallbackPanelObj.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Hides the conversation box
        /// </summary>
        public void HideConversationBox()
        {
            if (boxVisible)
            {
                boxVisible = false;
                var dialogManager = GameObject.Find("DialogueManager");
                if (dialogManager != null)
                {
                    EventManager.SendEventToGameObject("BOX DOWN", dialogManager, gameObject);
                }
                else if (UseFallbackCanvasIfNoDialogueManager && fallbackPanelObj != null)
                {
                    fallbackPanelObj.SetActive(false);
                }
            }
        }



        /// <summary>
        /// Immediately reveals the text in the dialogue box, rather than having to wait for it to type out
        /// </summary>
        public void FinishTyping()
        {
            var dialogManager = GameObject.Find("DialogueManager");
            if (dialogManager != null)
            {
                var text = dialogManager.transform.Find("Text")?.gameObject;
                if (text != null)
                {
                    var dialogueBox = text.GetComponent("DialogueBox");

                    var textMesh = (TextMeshPro)dialogueBox.ReflectGetField("textMesh", BindingFlags.Instance | BindingFlags.NonPublic);

                    if (textMesh != null)
                    {
                        textMesh.maxVisibleCharacters = 99999;
                    }
                    /*talking = true;
                    EventManager.SendEventToGameObject("TALK START", gameObject, gameObject);
                    StartDialogBoxConversation(, message);
                    yield return new WaitUntil(() => !talking);
                    EventManager.SendEventToGameObject("TALK END", gameObject, gameObject);*/
                }
                /*else
                {
                    yield return new WaitForSeconds(0.5f);
                }*/
            }
        }

        protected IEnumerator SpeakImmediate(string message)
        {
            Coroutine speakImmediateCoroutine = null;
            IEnumerator SpeakImmediateRoutine(Func<bool> finished)
            {
                while (!finished())
                {
                    FinishTyping();
                    yield return new WaitForFixedUpdate();
                }
                speakImmediateCoroutine = null;
            }

            ShowConversationBox();
            var dialogManager = GameObject.Find("DialogueManager");
            if (dialogManager != null)
            {
                var text = dialogManager.transform.Find("Text")?.gameObject;
                if (text != null)
                {
                    talking = true;
                    EventManager.SendEventToGameObject("TALK START", gameObject, gameObject);
                    StartDialogBoxConversation(text.GetComponent("DialogueBox"), message);
                    speakImmediateCoroutine = StartCoroutine(SpeakImmediateRoutine(() => !talking));
                    yield return new WaitUntil(() => !talking);
                    EventManager.SendEventToGameObject("TALK END", gameObject, gameObject);
                }
                else
                {
                    yield return new WaitForSeconds(0.5f);
                }
            }

            if (speakImmediateCoroutine != null)
            {
                StopCoroutine(speakImmediateCoroutine);
                speakImmediateCoroutine = null;
            }
        }

        /// <summary>
        /// Speaks a message to the player
        /// </summary>
        /// <param name="message">The message to speak</param>
        protected IEnumerator Speak(string message)
        {
            return Speak(message, (Action<int, string>)null);
        }

        /// <summary>
        /// Speaks a message to the player and optionally invokes a callback before showing the message
        /// </summary>
        /// <param name="message">The message to speak</param>
        /// <param name="onPageShown">Optional callback with page index and page text</param>
        protected IEnumerator Speak(string message, Action<int, string> onPageShown)
        {
            ShowConversationBox();
            onPageShown?.Invoke(0, message);
            var dialogManager = GameObject.Find("DialogueManager");
            if (dialogManager != null)
            {
                var text = dialogManager.transform.Find("Text")?.gameObject;
                if (text != null)
                {
                    talking = true;
                    EventManager.SendEventToGameObject("TALK START", gameObject, gameObject);
                    StartDialogBoxConversation(text.GetComponent("DialogueBox"), message);
                    yield return new WaitUntil(() => !talking);
                    EventManager.SendEventToGameObject("TALK END", gameObject, gameObject);
                }
                else
                {
                    yield return new WaitForSeconds(0.5f);
                }
            }
            else if (UseFallbackCanvasIfNoDialogueManager)
            {
                yield return SpeakWithFallbackCanvas(message);
            }
        }

        /// <summary>
        /// Speaks a series of messages to the player. Each message is split into seperate pages
        /// </summary>
        /// <param name="messages">The messages to speak</param>
        protected IEnumerator Speak(IEnumerable<string> messages)
        {
            return Speak(messages, null);
        }

        /// <summary>
        /// Speaks a series of messages to the player. Each message is split into seperate pages
        /// </summary>
        /// <param name="messages">The messages to speak</param>
        /// <param name="onPageShown">Optional callback with page index and page text</param>
        protected IEnumerator Speak(IEnumerable<string> messages, Action<int, string> onPageShown)
        {
            if (messages == null)
            {
                yield break;
            }

            int index = 0;
            foreach (var message in messages)
            {
                yield return Speak(message, (pageIndex, pageText) => onPageShown?.Invoke(index, pageText));
                index++;
            }
        }

        /// <summary>
        /// Speaks a series of messages to the player. Each message is split into seperate pages
        /// </summary>
        /// <param name="messages">The messages to speak</param>
        protected IEnumerator Speak(params string[] messages)
        {
            return Speak((IEnumerable<string>)messages);
        }

        /// <summary>
        /// Presents a yes/no question prompt to the player. The result is stored in <see cref="DialogBoxResult"/>
        /// </summary>
        /// <param name="message">The message to say in the prompt</param>
        protected IEnumerator PresentYesNoQuestion(string message)
        {
            return PresentYesNoQuestion(message, 0);
        }

        /// <summary>
        /// Presents a yes/no question prompt to the player. The result is stored in <see cref="DialogBoxResult"/>
        /// </summary>
        /// <param name="message">The message to speak</param>
        /// <param name="geoCost">How much geo the player need to pay to select "Yes"</param>
        protected IEnumerator PresentYesNoQuestion(string message, int geoCost)
        {
            HideConversationBox();
            var dialogManager = GameObject.Find("DialogueManager");
            if (dialogManager != null)
            {
                DialogBoxResult = YesNoResult.Neither;
                EventManager.SendEventToGameObject("BOX DOWN", dialogManager, gameObject);
                EventManager.SendEventToGameObject("TALK END", gameObject, gameObject);
                yield return new WaitForSeconds(0.3f);

                EventManager.SendEventToGameObject("BOX UP YN", dialogManager, gameObject);
                yield return new WaitForSeconds(0.25f);

                var ynPrompt = dialogManager.transform.Find("Text YN")?.gameObject;
                if (ynPrompt != null)
                {
                    PlayMakerUtilities.SetFsmInt(ynPrompt, "Dialogue Page Control", "Toll Cost", geoCost);
                    PlayMakerUtilities.SetFsmGameObject(ynPrompt, "Dialogue Page Control", "Requester", gameObject);
                    var dialogBox = ynPrompt.GetComponent("DialogueBox");
                    if (dialogBox != null)
                    {
                        StartDialogBoxConversation(dialogBox, message);

                        yield return new WaitUntil(() => DialogBoxResult != YesNoResult.Neither);
                        EventManager.SendEventToGameObject("BOX DOWN YN", dialogManager, gameObject);
                    }
                }
            }
            else if (UseFallbackCanvasIfNoDialogueManager)
            {
                yield return PresentYesNoWithFallbackCanvas(message);
            }
        }

        IEnumerator SpeakWithFallbackCanvas(string message)
        {
            EnsureFallbackCanvas();
            fallbackPanelObj.SetActive(true);

            var pages = SplitIntoPages(message);
            for (int i = 0; i < pages.Count; i++)
            {
                fallbackMainText.text = pages[i];
                fallbackPromptText.text = i < pages.Count - 1 ? "Press Enter to continue" : "Press Enter to finish";
                yield return WaitForAdvanceInput();
            }
        }

        IEnumerator PresentYesNoWithFallbackCanvas(string message)
        {
            EnsureFallbackCanvas();
            fallbackPanelObj.SetActive(true);

            DialogBoxResult = YesNoResult.Neither;

            int selected = 0; // 0 = yes, 1 = no
            bool done = false;
            while (!done)
            {
                fallbackMainText.text = message;
                fallbackPromptText.text = selected == 0
                    ? "> Yes    No\nArrow Keys to change, Enter to confirm"
                    : "  Yes  > No\nArrow Keys to change, Enter to confirm";

                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    selected = 0;
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    selected = 1;
                }
                else if (IsAdvancePressed())
                {
                    DialogBoxResult = selected == 0 ? YesNoResult.Yes : YesNoResult.No;
                    done = true;
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    DialogBoxResult = YesNoResult.Neither;
                    done = true;
                }

                yield return null;
            }

            yield return new WaitForSeconds(FallbackInputDebounceTime);
        }

        IEnumerator WaitForAdvanceInput()
        {
            while (!IsAdvancePressed())
            {
                yield return null;
            }
            yield return new WaitForSeconds(FallbackInputDebounceTime);
        }

        bool IsAdvancePressed()
        {
            return Input.GetKeyDown(FallbackAdvanceKey)
                || Input.GetKeyDown(KeyCode.KeypadEnter)
                || Input.GetKeyDown(KeyCode.Z)
                || Input.GetKeyDown(KeyCode.X);
        }

        static List<string> SplitIntoPages(string message)
        {
            var results = new List<string>();
            if (string.IsNullOrEmpty(message))
            {
                results.Add(string.Empty);
                return results;
            }

            var pages = message.Split(new[] { "<page>" }, StringSplitOptions.None);
            for (int i = 0; i < pages.Length; i++)
            {
                results.Add(pages[i]);
            }
            return results;
        }

        void EnsureFallbackCanvas()
        {
            if (fallbackCanvasObj != null)
            {
                return;
            }

            fallbackCanvasObj = new GameObject("Conversation Fallback Canvas");
            var canvas = fallbackCanvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            fallbackCanvasObj.AddComponent<GraphicRaycaster>();

            var scaler = fallbackCanvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            fallbackPanelObj = new GameObject("Panel");
            fallbackPanelObj.transform.SetParent(fallbackCanvasObj.transform, false);
            var panelImage = fallbackPanelObj.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.82f);
            var panelRect = fallbackPanelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.08f, 0.06f);
            panelRect.anchorMax = new Vector2(0.92f, 0.34f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            fallbackMainText = CreateFallbackText("MainText", fallbackPanelObj.transform, 40, TextAnchor.UpperLeft);
            var mainRect = fallbackMainText.rectTransform;
            mainRect.anchorMin = new Vector2(0f, 0.28f);
            mainRect.anchorMax = new Vector2(1f, 1f);
            mainRect.offsetMin = new Vector2(0f, 0f);
            mainRect.offsetMax = new Vector2(0f, 0f);
            fallbackMainText.fontSize = 42;
            fallbackMainText.horizontalOverflow = HorizontalWrapMode.Wrap;
            fallbackMainText.verticalOverflow = VerticalWrapMode.Overflow;

            fallbackPromptText = CreateFallbackText("PromptText", fallbackPanelObj.transform, 30, TextAnchor.LowerLeft);
            var promptRect = fallbackPromptText.rectTransform;
            promptRect.anchorMin = new Vector2(0f, 0f);
            promptRect.anchorMax = new Vector2(1f, 0.28f);
            promptRect.offsetMin = new Vector2(0f, 0f);
            promptRect.offsetMax = new Vector2(0f, 0f);
            fallbackPromptText.fontSize = 30;
            fallbackPromptText.horizontalOverflow = HorizontalWrapMode.Wrap;
            fallbackPromptText.verticalOverflow = VerticalWrapMode.Overflow;
        }

        static Text CreateFallbackText(string objectName, Transform parent, int fontSize, TextAnchor alignment)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.text = string.Empty;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = Color.white;
            return text;
        }

        void DestroyFallbackCanvas()
        {
            if (fallbackCanvasObj != null)
            {
                Destroy(fallbackCanvasObj);
                fallbackCanvasObj = null;
                fallbackPanelObj = null;
                fallbackMainText = null;
                fallbackPromptText = null;
            }
        }

        static void StartDialogBoxConversation(object dialogBox, string text)
        {
            var type = dialogBox.GetType();
            type.GetField("currentConversation").SetValue(dialogBox, text.GetHashCode().ToString());
            type.GetField("currentPage").SetValue(dialogBox, 1);
            var textMesh = (TextMeshPro)type.GetField("textMesh",BindingFlags.NonPublic | BindingFlags.Instance).GetValue(dialogBox);
            textMesh.text = text;
            textMesh.ForceMeshUpdate();
            type.GetMethod("ShowPage").Invoke(dialogBox, new object[] { 1 });
        }

        /// <summary>
        /// Display an <see cref="WeaverBossTitle"/>. This is mainly used to display the name of the NPC the player is talking to
        /// </summary>
        /// <param name="title"></param>
        /// <param name="minorText"></param>
        protected void DisplayTitle(string title, string minorText = "")
        {
            WeaverBossTitle.Spawn(minorText,title);
        }
    }
}
