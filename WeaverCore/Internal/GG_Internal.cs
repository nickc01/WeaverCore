using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Internal
{
    internal static class GG_Internal
    {
        internal static GameObject mageKnightStatue;

        internal static GameObject bossUIPrefab;

        internal static GameObject attuned_award_prefab;
        internal static GameObject ascended_award_prefab;
        internal static GameObject radiant_award_prefab;

        internal static AudioSource AudioPlayerPrefab;
        internal static GameObject StrikeNailR;

        internal static GameObject ggBattleTransitions;

        internal static AudioEvent statueDownSound;
        internal static AudioEvent statueUpSound;

        internal static AudioEvent BossLeverSwitchSound;

        internal static GameObject dreamImpactPrefab;
        internal static GameObject dreamBurstEffectPrefab;
        internal static GameObject dreamBurstEffectOffPrefab;

        internal static GameObject dreamAreaEffect;

        internal static RuntimeAnimatorController[] statueAnimators;


        public static void SetMageKnightStatue(GameObject preloadedStatue)
        {
            mageKnightStatue = preloadedStatue;

            var bossStatue = preloadedStatue.GetComponent<BossStatue>();

            statueDownSound = bossStatue.statueDownSound;
            statueUpSound = bossStatue.statueUpSound;

            statueAnimators = mageKnightStatue.GetComponentsInChildren<Animator>(true).Select(a => a.runtimeAnimatorController).ToArray();

            //WeaverLog.Log("A");
            var trophyPlaque = mageKnightStatue.GetComponentInChildren<BossStatueTrophyPlaque>();

            if (trophyPlaque != null)
            {
                attuned_award_prefab = trophyPlaque.tierCompleteEffectPrefabs[0];
                ascended_award_prefab = trophyPlaque.tierCompleteEffectPrefabs[1];
                radiant_award_prefab = trophyPlaque.tierCompleteEffectPrefabs[2];
            }
            //WeaverLog.Log("B");

            var statueLever = mageKnightStatue.GetComponentInChildren<BossStatueLever>(true);

            //WeaverLog.Log("C");
            AudioPlayerPrefab = statueLever.audioPlayerPrefab;
            StrikeNailR = statueLever.strikeNailPrefab;
            BossLeverSwitchSound = statueLever.switchSound;

            var statueDreamLever = mageKnightStatue.GetComponentInChildren<BossStatueDreamToggle>(true);

            dreamImpactPrefab = statueDreamLever.dreamImpactPrefab;
            dreamBurstEffectPrefab = statueDreamLever.dreamBurstEffectPrefab;
            dreamBurstEffectOffPrefab = statueDreamLever.dreamBurstEffectOffPrefab;

            //WeaverLog.Log("D");
            var inspectObject = mageKnightStatue.transform.Find("Inspect");

            //WeaverLog.Log("E");
            var playMakerFSMType = inspectObject.GetComponent("PlayMakerFSM").GetType();
            //WeaverLog.Log("F");
            //MonoBehaviour playMakerFSM = null;
            object fsm = null;

            //WeaverLog.Log("G");
            foreach (var pmFSM in inspectObject.GetComponents(playMakerFSMType))
            {
                fsm = pmFSM.ReflectGetField("fsm");
                if ((string)fsm.ReflectGetProperty("Name") == "GG Boss UI")
                {
                    break;
                }
                else
                {
                    fsm = null;
                }
            }

            //WeaverLog.Log("H");

            var states = (object[])fsm.ReflectGetField("states");

            //WeaverLog.Log("I");

            for (int i = 0; i < states.Length; i++)
            {
                var state = states[i];
                var name = (string)state.ReflectGetProperty("Name");
                if (name == "Open UI")
                {
                    var actionData = state.ReflectGetField("actionData");

                    var fsmGameObjectParams = (IEnumerable)actionData.ReflectGetField("fsmGameObjectParams");

                    foreach (var gm in fsmGameObjectParams)
                    {
                        var value = (GameObject)gm.ReflectGetProperty("Value");

                        if (value.name == "GG_Challenge_Statue_Canvas")
                        {
                            bossUIPrefab = value;
                            break;
                        }
                    }


                    //break;
                }
                else if (name == "Impact")
                {
                    var actionData = state.ReflectGetField("actionData");

                    var fsmGameObjectParams = (IEnumerable)actionData.ReflectGetField("fsmGameObjectParams");

                    foreach (var gm in fsmGameObjectParams)
                    {
                        var value = (GameObject)gm.ReflectGetProperty("Value");

                        if (value.name == "dream_area_effect")
                        {
                            dreamAreaEffect = value;
                            break;
                        }
                    }
                }

                if (bossUIPrefab != null && dreamAreaEffect != null)
                {
                    break;
                }
            }

            //WeaverLog.Log("J");

            var doorObject = mageKnightStatue.GetComponentInChildren<TransitionPoint>(true);

            //WeaverLog.Log("K");

            object doorFSM = null;

            //WeaverLog.Log("L");
            foreach (var pmFSM in doorObject.GetComponents(playMakerFSMType))
            {
                doorFSM = pmFSM.ReflectGetField("fsm");
                if ((string)doorFSM.ReflectGetProperty("Name") == "Return from boss")
                {
                    break;
                }
                else
                {
                    doorFSM = null;
                }
            }

            //WeaverLog.Log("M");

            var doorStates = (object[])doorFSM.ReflectGetField("states");

            //WeaverLog.Log("N");

            for (int i = 0; i < doorStates.Length; i++)
            {
                var state = doorStates[i];
                if ((string)state.ReflectGetProperty("Name") == "Transition Wait")
                {
                    var actionData = state.ReflectGetField("actionData");

                    var fsmGameObjectParams = (IEnumerable)actionData.ReflectGetField("fsmGameObjectParams");

                    foreach (var gm in fsmGameObjectParams)
                    {
                        var value = (GameObject)gm.ReflectGetProperty("Value");

                        if (value.name == "gg_battle_transitions")
                        {
                            ggBattleTransitions = value;
                            break;
                        }
                    }


                    break;
                }
            }


            GameObject.Destroy(mageKnightStatue);
            //WeaverLog.Log("O");
        }
    }
}