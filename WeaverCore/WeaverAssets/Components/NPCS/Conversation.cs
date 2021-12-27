using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
    public abstract class Conversation : MonoBehaviour
    {
        public enum YesNoResult
        {
            Neither,
            Yes,
            No
        }

        bool talking = false;

        public YesNoResult DialogBoxResult { get; private set; }

        EventManager eventReceiver;

        bool boxVisible = false;

        protected abstract IEnumerator DoConversation();

        public void StartConversation()
        {
            StartCoroutine(StartConversationRoutine());
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

        public IEnumerator StartConversationRoutine()
        {
            if (eventReceiver == null)
            {
                if (!TryGetComponent(out eventReceiver))
                {
                    eventReceiver = gameObject.AddComponent<EventManager>();
                }
                eventReceiver.OnReceivedEvent += EventReceiver_OnReceivedEvent;
            }
            HeroUtilities.PlayPlayerClip("LookUp");
            var dialogManager = GameObject.Find("DialogueManager");
            if (dialogManager != null)
            {
                EventManager.SendEventToGameObject("BOX UP", dialogManager, gameObject);
            }
            boxVisible = true;
            yield return new WaitForSeconds(0.3f);
            yield return DoConversation();

        }

        protected void ShowConversationBox()
        {
            if (!boxVisible)
            {
                boxVisible = true;
                var dialogManager = GameObject.Find("DialogueManager");
                if (dialogManager != null)
                {
                    EventManager.SendEventToGameObject("BOX UP", dialogManager, gameObject);
                }
            }
        }

        protected void HideConversationBox()
        {
            if (boxVisible)
            {
                boxVisible = false;
                var dialogManager = GameObject.Find("DialogueManager");
                if (dialogManager != null)
                {
                    EventManager.SendEventToGameObject("BOX DOWN", dialogManager, gameObject);
                }
            }
        }

        /*protected IEnumerator Speak(string langKey, string sheetTitle = "Dialogue")
        {
            return Speak(WeaverLanguage.GetString(langKey, sheetTitle,$"ERROR GETTING TRANSLATION: LANG_KEY = {langKey}, SHEET_TITLE = {sheetTitle}"));
        }*/

        protected IEnumerator Speak(string message)
        {
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
                    yield return new WaitUntil(() => !talking);
                    HideConversationBox();
                    EventManager.SendEventToGameObject("TALK END", gameObject, gameObject);
                }
            }
        }

        protected IEnumerator Speak(IEnumerable<string> messages)
        {
            string insert = "<page>";
            StringBuilder finalMessage = new StringBuilder();
            foreach (var msg in messages)
            {
                finalMessage.Append(msg);
                finalMessage.Append(insert);
            }
            finalMessage.Remove(finalMessage.Length - insert.Length, insert.Length);
            return Speak(finalMessage.ToString());
        }

        protected IEnumerator Speak(params string[] messages)
        {
            return Speak((IEnumerable<string>)messages);
        }

        protected IEnumerator PresentYesNoQuestion(string message)
        {
            return PresentYesNoQuestion(message, 0);
        }

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
        }

        static void StartDialogBoxConversation(object dialogBox, string text)
        {
            WeaverLog.Log("CONV1");
            var type = dialogBox.GetType();
            WeaverLog.Log("CONV2");
            type.GetField("currentConversation").SetValue(dialogBox, text.GetHashCode().ToString());
            WeaverLog.Log("CONV3");
            type.GetField("currentPage").SetValue(dialogBox, 1);
            WeaverLog.Log("CONV4");
            var textMesh = (TextMeshPro)type.GetField("textMesh",BindingFlags.NonPublic | BindingFlags.Instance).GetValue(dialogBox);
            WeaverLog.Log("CONV5");
            textMesh.text = text;
            WeaverLog.Log("CONV6");
            textMesh.ForceMeshUpdate();
            WeaverLog.Log("CONV7");
            type.GetMethod("ShowPage").Invoke(dialogBox, new object[] { 1 });
            WeaverLog.Log("CONV8");
        }

        protected void DisplayTitle(string title, string minorText = "")
        {
            AreaTitle.Spawn(title,minorText);
        }
    }
}
