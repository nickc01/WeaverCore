using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Implementations;
using WeaverCore.Internal;

namespace WeaverCore.Game.Implementations
{
    public class G_HunterJournal_I : HunterJournal_I
    {
        /*[OnInit(0)]
        private static void Init()
        {
            updateMessageInstance = typeof(EnemyDeathEffects).GetField("journalUpdateMessageSpawned", BindingFlags.Static | BindingFlags.NonPublic);
            On.EnemyDeathEffects.PreInstantiate += EnemyDeathEffects_PreInstantiate;
            getPrefabField = typeof(EnemyDeathEffects).GetField("journalUpdateMessagePrefab", BindingFlags.Instance | BindingFlags.NonPublic);
        }*/

        /*private static void EnemyDeathEffects_PreInstantiate(On.EnemyDeathEffects.orig_PreInstantiate orig, EnemyDeathEffects self)
        {
            orig(self);
            GameObject gameObject = (GameObject)getPrefabField.GetValue(self);
            bool flag = gameObject != null;
            if (flag)
            {
                JournalUpdatePrefab = gameObject;
            }
        }*/

        private static GameObject SpawnJournalUpdate()
        {
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Other_Preloads.JournalUpdateMessagePrefab);
            gameObject.SetActive(false);
            return gameObject;
        }

        public override void DisplayJournalUpdate(bool displayText)
        {
            if (updateMessageInstance == null)
            {
                updateMessageInstance = typeof(EnemyDeathEffects).GetField("journalUpdateMessageSpawned", BindingFlags.Static | BindingFlags.NonPublic);
            }
            GameObject gameObject = (GameObject)updateMessageInstance.GetValue(null);
            bool flag = gameObject == null;
            if (flag)
            {
                gameObject = SpawnJournalUpdate();
                updateMessageInstance.SetValue(null, gameObject);
            }
            bool activeSelf = gameObject.activeSelf;
            if (activeSelf)
            {
                gameObject.SetActive(false);
            }
            gameObject.SetActive(true);
            PlayMakerFSM playMakerFSM = PlayMakerFSM.FindFsmOnGameObject(gameObject, "Journal Msg");
            bool flag2 = playMakerFSM;
            if (flag2)
            {
                FSMUtility.SetBool(playMakerFSM, "Full", displayText);
                FSMUtility.SetBool(playMakerFSM, "Should Recycle", true);
            }
        }

        public override bool HasKilled(string name)
        {
            PlayerData playerData = GameManager.instance.playerData;
            string boolName = "killed" + name;
            return playerData.GetBool(boolName);
        }

        public override int KillsLeft(string name)
        {
            PlayerData playerData = GameManager.instance.playerData;
            string intName = "kills" + name;
            int num = playerData.GetInt(intName);
            bool flag = num < 0;
            if (flag)
            {
                num = 0;
            }
            return num;
        }

        public override void RecordKillFor(string name)
        {
            PlayerData playerData = GameManager.instance.playerData;
            string hasBeenKilledConvo = "killed" + name;
            string killCountConvo = "kills" + name;
            string isNewEntryConvo = "newData" + name;
            //bool flag = false;
            bool isNewKill = !playerData.GetBool(hasBeenKilledConvo);
            if (isNewKill)
            {
                //flag = true;
                playerData.SetBool(hasBeenKilledConvo, true);
                playerData.SetBool(isNewEntryConvo, true);
            }
            bool entryCompleted = false;
            int killsLeft = playerData.GetInt(killCountConvo);
            if (killsLeft > 0)
            {
                killsLeft--;
                playerData.SetInt(killCountConvo, killsLeft);
                if (killsLeft <= 0)
                {
                    entryCompleted = true;
                }
            }
            if (playerData.GetBool("hasJournal"))
            {
                bool displayJournalUpdateMessage = false;
                if (entryCompleted)
                {
                    displayJournalUpdateMessage = true;
                    playerData.SetInt("journalEntriesCompleted", playerData.GetInt("journalEntriesCompleted") + 1);
                }
                else
                {
                    if (isNewKill)
                    {
                        displayJournalUpdateMessage = true;
                        playerData.SetInt("journalNotesCompleted", playerData.GetInt("journalNotesCompleted") + 1);
                    }
                }
                if (displayJournalUpdateMessage)
                {
                    DisplayJournalUpdate(entryCompleted);
                }
            }
        }

        public override bool HasEntryFor(string name)
        {
            PlayerData playerData = GameManager.instance.playerData;
            string name2 = "killed" + name;
            return typeof(PlayerData).GetField(name2, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;
        }

        //private static GameObject JournalUpdatePrefab;

        private static FieldInfo updateMessageInstance;

        //private static FieldInfo getPrefabField;
    }
}
