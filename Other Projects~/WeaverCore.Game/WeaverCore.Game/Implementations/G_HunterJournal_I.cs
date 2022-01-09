using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
    public class G_HunterJournal_I : HunterJournal_I
    {
        [OnInit(0)]
        private static void Init()
        {
            updateMessageInstance = typeof(EnemyDeathEffects).GetField("journalUpdateMessageSpawned", BindingFlags.Static | BindingFlags.NonPublic);
            On.EnemyDeathEffects.PreInstantiate += EnemyDeathEffects_PreInstantiate;
            getPrefabField = typeof(EnemyDeathEffects).GetField("journalUpdateMessagePrefab", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private static void EnemyDeathEffects_PreInstantiate(On.EnemyDeathEffects.orig_PreInstantiate orig, EnemyDeathEffects self)
        {
            orig(self);
            GameObject gameObject = (GameObject)getPrefabField.GetValue(self);
            bool flag = gameObject != null;
            if (flag)
            {
                JournalUpdatePrefab = gameObject;
            }
        }

        private static GameObject SpawnJournalUpdate()
        {
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(JournalUpdatePrefab);
            gameObject.SetActive(false);
            return gameObject;
        }

        public override void DisplayJournalUpdate(bool displayText)
        {
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
            string boolName = "killed" + name;
            string intName = "kills" + name;
            string boolName2 = "newData" + name;
            bool flag = false;
            bool flag2 = !playerData.GetBool(boolName);
            if (flag2)
            {
                flag = true;
                playerData.SetBool(boolName, true);
                playerData.SetBool(boolName2, true);
            }
            bool flag3 = false;
            int num = playerData.GetInt(intName);
            bool flag4 = num > 0;
            if (flag4)
            {
                num--;
                playerData.SetInt(intName, num);
                bool flag5 = num <= 0;
                if (flag5)
                {
                    flag3 = true;
                }
            }
            bool @bool = playerData.GetBool("hasJournal");
            if (@bool)
            {
                bool flag6 = false;
                bool flag7 = flag3;
                if (flag7)
                {
                    flag6 = true;
                    playerData.SetInt("journalEntriesCompleted", playerData.GetInt("journalEntriesCompleted") + 1);
                }
                else
                {
                    bool flag8 = flag;
                    if (flag8)
                    {
                        flag6 = true;
                        playerData.SetInt("journalNotesCompleted", playerData.GetInt("journalNotesCompleted") + 1);
                    }
                }
                bool flag9 = flag6;
                if (flag9)
                {
                    DisplayJournalUpdate(flag3);
                }
            }
        }

        public override bool HasEntryFor(string name)
        {
            PlayerData playerData = GameManager.instance.playerData;
            string name2 = "killed" + name;
            return typeof(PlayerData).GetField(name2, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;
        }

        private static GameObject JournalUpdatePrefab;

        private static FieldInfo updateMessageInstance;

        private static FieldInfo getPrefabField;
    }
}
