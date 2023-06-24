using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Patches
{

    static class Application_Patches
    {
        static Func<BossChallengeUI, BossStatue> bossStatue;

        [OnRuntimeInit]
        static void Init()
        {
            bossStatue = ReflectionUtilities.CreateFieldGetter<BossChallengeUI, BossStatue>("bossStatue");

            On.BossChallengeUI.LoadBoss_int_bool += BossChallengeUI_LoadBoss_int_bool;
        }

        /*[OnHarmonyPatch]
        static void Patch(HarmonyPatcher patcher)
        {
            var transitionToSnapshot = typeof(AudioMixer).GetMethod("TransitionToSnapshot", BindingFlags.NonPublic | BindingFlags.Instance);

            var transitionToSnapshotPrefix = typeof(Application_Patches).GetMethod("TransitionToSnapshot_Prefix", BindingFlags.NonPublic | BindingFlags.Static);

            patcher.Patch(transitionToSnapshot, transitionToSnapshotPrefix, null);
        }


        private static bool TransitionToSnapshot_Prefix(AudioMixer __instance, AudioMixerSnapshot snapshot, float timeToReach)
        {
            try
            {
                if (snapshot.audioMixer.name == "Actors")
                {
                    WeaverLog.Log("SNAPSHOT TRANSITION");
                    WeaverLog.Log("Snapshot = " + snapshot.name);
                    WeaverLog.Log("Mixer = " + snapshot.audioMixer.name);
                    WeaverLog.Log("Mixer Group = " + snapshot.audioMixer.outputAudioMixerGroup.name);
                }
            }
            catch (Exception e)
            {
                WeaverLog.LogException(e);
            }
            return true;
        }*/

        private static void BossChallengeUI_LoadBoss_int_bool(On.BossChallengeUI.orig_LoadBoss_int_bool orig, BossChallengeUI self, int level, bool doHideAnim)
        {
            //WeaverLog.Log("PRE_A_A");
            BossScene bossScene = (bossStatue(self).UsingDreamVersion ? bossStatue(self).dreamBossScene : bossStatue(self).bossScene);
            //WeaverLog.Log("PRE_A_B");
            string text = bossScene.sceneName;
            //WeaverLog.Log("PRE_A_C");
            switch (level)
            {
                case 0:
                    text = bossScene.Tier1Scene;
                    break;
                case 1:
                    text = bossScene.Tier2Scene;
                    break;
                case 2:
                    text = bossScene.Tier3Scene;
                    break;
            }
            //WeaverLog.Log("PRE_A_D");
            StaticVariableList.SetValue("bossSceneToLoad", text);
            //WeaverLog.Log("PRE_A_E");
            BossStatueLoadManager.RecordBossScene(bossScene);
            //WeaverLog.Log("PRE_A_F");

            /*foreach (var field in typeof(BossChallengeUI).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                WeaverLog.Log("Field = " + field.Name);
            }*/

            var onCancelField = typeof(BossChallengeUI).GetField("OnCancel", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //WeaverLog.Log("PRE_A_G");
            onCancelField.SetValue(self, null);

            //typeof(BossChallengeUI).GetEvent("OnCancel").
            //self.OnCancel = null;
            //WeaverLog.Log("A_A");
            self.Hide(doHideAnim);
            //WeaverLog.Log("A_B");
            PlayerData.instance.SetString("bossReturnEntryGate", bossStatue(self).dreamReturnGate.name);
            //GameManager.instance.playerData.bossReturnEntryGate = bossStatue(self).dreamReturnGate.name;
            //WeaverLog.Log("A_C");
            BossSceneController.SetupEvent = delegate (BossSceneController controller)
            {
                //WeaverLog.Log("SETUP EVENT A");
                controller.BossLevel = level;
                //WeaverLog.Log("SETUP EVENT B");
                controller.DreamReturnEvent = "DREAM RETURN";
                //WeaverLog.Log("SETUP EVENT C");
                controller.OnBossesDead += delegate
                {
                    //WeaverLog.Log("Bosses Dead A");
                    string fieldName = (bossStatue(self).UsingDreamVersion ? bossStatue(self).dreamStatueStatePD : bossStatue(self).statueStatePD);
                    //WeaverLog.Log("Bosses Dead B");
                    BossStatue.Completion playerDataVariable = GameManager.instance.GetPlayerDataVariable<BossStatue.Completion>(fieldName);
                    //WeaverLog.Log("Bosses Dead C");
                    switch (level)
                    {
                        case 0:
                            playerDataVariable.completedTier1 = true;
                            break;
                        case 1:
                            playerDataVariable.completedTier2 = true;
                            break;
                        case 2:
                            playerDataVariable.completedTier3 = true;
                            break;
                    }
                    //WeaverLog.Log("Bosses Dead D");
                    //WeaverLog.Log("SETTING STATUE DATA VARIABLE TO = " + JsonUtility.ToJson(playerDataVariable, true));
                    GameManager.instance.SetPlayerDataVariable(fieldName, playerDataVariable);
                    PlayerData.instance.SetString("currentBossStatueCompletionKey", bossStatue(self).UsingDreamVersion ? bossStatue(self).dreamStatueStatePD : bossStatue(self).statueStatePD);
                    //GameManager.instance.playerData.currentBossStatueCompletionKey = (bossStatue(self).UsingDreamVersion ? bossStatue(self).dreamStatueStatePD : bossStatue(self).statueStatePD);
                    PlayerData.instance.SetInt("bossStatueTargetLevel", level);
                    //WeaverLog.Log("Bosses Dead E");
                    //GameManager.instance.playerData.bossStatueTargetLevel = level;
                };
                //WeaverLog.Log("SETUP EVENT D");
                controller.OnBossSceneComplete += delegate
                {
                    //WeaverLog.Log("Boss Scene Complete A");
                    controller.DoDreamReturn();
                    //WeaverLog.Log("Boss Scene Complete B");
                };
                //WeaverLog.Log("SETUP EVENT E");
            };
            //WeaverLog.Log("A_D");

            var onLevelSelectedField = typeof(BossChallengeUI).GetField("OnLevelSelected", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            //WeaverLog.Log("A_E");
            if (onLevelSelectedField.GetValue(self) != null)
            {
                //WeaverLog.Log("A_F");
                foreach (var func in ((Delegate)onLevelSelectedField.GetValue(self)).GetInvocationList())
                {
                    func.DynamicInvoke(null);
                }
                //WeaverLog.Log("A_G");
            }

            //WeaverLog.Log("A_H");

            /*if (self.OnLevelSelected != null)
            {
                self.OnLevelSelected();
            }*/
        }
    }

}
