using HutongGames.PlayMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Implementations;
using WeaverCore.Internal;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Implementations
{
    public class G_DreamnailUtilities_I : DreamnailUtilities_I
    {
        [OnRuntimeInit]
        static void Init()
        {
            EventManager.OnEventTriggered += EventManager_OnEventTriggered;
            //On.DialogueBox.SendConvEndEvent += DialogueBox_SendConvEndEvent;

            //onConvoFinish += G_DreamnailUtilities_I_onConvoFinish;
        }

        //static event Action<DialogueBox> onConvoFinish;

        static DialogueBox dreamBox;

        /*private static void DialogueBox_SendConvEndEvent(On.DialogueBox.orig_SendConvEndEvent orig, DialogueBox self)
        {
            orig(self);
            onConvoFinish?.Invoke(self);
        }*/

        static GameObject _dreamImpact = null;
        static GameObject _dreamImpactPt = null;
        static GameObject _dreamActivePt = null;

        static List<ParticleSystem> activePTs = new List<ParticleSystem>();

        static UnboundCoroutine currentDreamnailRoutine = null;

        public override void DisplayEnemyDreamMessage(int convoAmount, string convoTitle, string sheetTitle)
        {
            PlayMakerFSM playMakerFSM = PlayMakerFSM.FindFsmOnGameObject(FsmVariables.GlobalVariables.GetFsmGameObject("Enemy Dream Msg").Value, "Display");
            playMakerFSM.FsmVariables.GetFsmInt("Convo Amount").Value = convoAmount;
            playMakerFSM.FsmVariables.GetFsmString("Convo Title").Value = convoTitle;
            playMakerFSM.SendEvent("DISPLAY ENEMY DREAM");
        }

        public override void DisplayRegularDreamMessage(string convoTitle, string sheetTitle)
        {
            if (currentDreamnailRoutine != null)
            {
                UnboundCoroutine.Stop(currentDreamnailRoutine);
                currentDreamnailRoutine = null;
            }
            currentDreamnailRoutine = UnboundCoroutine.Start(DreamnailRoutine(convoTitle, sheetTitle));
        }

        static bool dreamConvoDone = false;

        IEnumerator DreamnailRoutine(string convoTitle, string sheetName)
        {
            PlayerData.instance.SetBool("disablePause", true);
            HeroController.instance.RelinquishControl();
            PlayMakerUtilities.SetFsmBool(HeroController.instance.gameObject, "Dream Nail", "Dream Convo", true);

            EventManager.BroadcastEvent("DREAM DIALOGUE START", null);

            var dialogManager = GameObject.Find("DialogueManager");

            if (dialogManager != null)
            {
                EventManager.SendEventToGameObject("BOX UP DREAM", dialogManager);
            }

            yield return new WaitForSeconds(0.3f);

            if (dialogManager != null)
            {
                var text = dialogManager.transform.Find("Text");

                if (text != null)
                {
                    var dialogBoxComp = text.GetComponent<DialogueBox>();

                    if (dreamBox == null)
                    {
                        dreamBox = dialogBoxComp;
                    }

                    dreamConvoDone = false;

                    dialogBoxComp.StartConversation(convoTitle, sheetName);

                    yield return new WaitUntil(() => dreamConvoDone);

                    EventManager.SendEventToGameObject("BOX DOWN DREAM", dialogManager);
                }
            }

            EventManager.BroadcastEvent("DREAM AREA DISABLE", null);

            yield return new WaitForSeconds(0.3f);

            PlayerData.instance.SetBool("disablePause", false);
            HeroController.instance.RegainControl();
            HeroController.instance.StartAnimationControl();
            EventManager.BroadcastEvent("DREAM DIALOGUE END", null);

            yield break;
        }

        public override void CancelRegularDreamnailMessage()
        {
            var dialogManager = GameObject.Find("DialogueManager");
            if (dialogManager != null)
            {
                EventManager.SendEventToGameObject("BOX DOWN DREAM", dialogManager);
            }

            EventManager.BroadcastEvent("DREAM AREA DISABLE", null);

            HeroController.instance.RegainControl();
            HeroController.instance.StartAnimationControl();

            EventManager.BroadcastEvent("DREAM DIALOGUE END", null);
        }

        public override void PlayDreamnailEffects(Vector3 position)
        {
            var areaEffect = GameObject.Instantiate(Other_Preloads.dream_area_effectPrefab, position, Quaternion.identity);

            if (_dreamImpact == null)
            {
                _dreamImpact = WeaverAssets.LoadWeaverAsset<GameObject>("Dream Impact Prefab");
            }

            if (_dreamImpactPt == null)
            {
                _dreamImpactPt = WeaverAssets.LoadWeaverAsset<GameObject>("Dream Impact Pt");
            }

            if (_dreamActivePt == null)
            {
                _dreamActivePt = WeaverAssets.LoadWeaverAsset<GameObject>("Dream Active Pt");
            }

            Pooling.Instantiate(_dreamImpact, position, Quaternion.identity);

            var impactPtInstance = Pooling.Instantiate(_dreamImpactPt, position + _dreamImpactPt.transform.localPosition, _dreamImpactPt.transform.rotation);

            impactPtInstance.GetComponent<ParticleSystem>().Play();


            var activePtInstance = Pooling.Instantiate(_dreamActivePt, position + _dreamActivePt.transform.localPosition, _dreamActivePt.transform.rotation);

            activePTs.RemoveAll(pt => pt == null);

            var activePtParticles = activePtInstance.GetComponent<ParticleSystem>();
            activePtParticles.Play();

            activePTs.Add(activePtParticles);

            CameraShaker.Instance.Shake(Enums.ShakeType.AverageShake);
        }

        private static void EventManager_OnEventTriggered(string eventName, GameObject source, GameObject destination, EventManager.EventType eventType)
        {
            if (eventName == "DREAM DIALOGUE END" || eventName == "CONVO_FINISH")
            {
                foreach (var activePT in activePTs)
                {
                    activePT.Stop();
                }

                activePTs.Clear();
            }

            if (eventName == "CONVO_FINISH")
            {
                if (dreamBox != null && source == dreamBox.gameObject)
                {
                    dreamConvoDone = true;
                }
            }
        }
    }
}