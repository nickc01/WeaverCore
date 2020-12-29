using System;
using System.Reflection;
using On;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
	// Token: 0x0200000D RID: 13
	public class G_HunterJournal_I : HunterJournal_I
	{
		// Token: 0x0600002A RID: 42 RVA: 0x00002CA8 File Offset: 0x00000EA8
		[OnInit(0)]
		private static void Init()
		{
			G_HunterJournal_I.updateMessageInstance = typeof(global::EnemyDeathEffects).GetField("journalUpdateMessageSpawned", BindingFlags.Static | BindingFlags.NonPublic);
			On.EnemyDeathEffects.PreInstantiate += G_HunterJournal_I.EnemyDeathEffects_PreInstantiate;
			G_HunterJournal_I.getPrefabField = typeof(global::EnemyDeathEffects).GetField("journalUpdateMessagePrefab", BindingFlags.Instance | BindingFlags.NonPublic);
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00002D00 File Offset: 0x00000F00
		private static void EnemyDeathEffects_PreInstantiate(On.EnemyDeathEffects.orig_PreInstantiate orig, global::EnemyDeathEffects self)
		{
			orig(self);
			GameObject gameObject = (GameObject)G_HunterJournal_I.getPrefabField.GetValue(self);
			bool flag = gameObject != null;
			if (flag)
			{
				G_HunterJournal_I.JournalUpdatePrefab = gameObject;
			}
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002D3C File Offset: 0x00000F3C
		private static GameObject SpawnJournalUpdate()
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(G_HunterJournal_I.JournalUpdatePrefab);
			gameObject.SetActive(false);
			return gameObject;
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00002D64 File Offset: 0x00000F64
		public override void DisplayJournalUpdate(bool displayText)
		{
			GameObject gameObject = (GameObject)G_HunterJournal_I.updateMessageInstance.GetValue(null);
			bool flag = gameObject == null;
			if (flag)
			{
				gameObject = G_HunterJournal_I.SpawnJournalUpdate();
				G_HunterJournal_I.updateMessageInstance.SetValue(null, gameObject);
			}
			bool activeSelf = gameObject.activeSelf;
			if (activeSelf)
			{
				gameObject.SetActive(false);
			}
			gameObject.SetActive(true);
			global::PlayMakerFSM playMakerFSM = global::PlayMakerFSM.FindFsmOnGameObject(gameObject, "Journal Msg");
			bool flag2 = playMakerFSM;
			if (flag2)
			{
				global::FSMUtility.SetBool(playMakerFSM, "Full", displayText);
				global::FSMUtility.SetBool(playMakerFSM, "Should Recycle", true);
			}
		}

		// Token: 0x0600002E RID: 46 RVA: 0x00002DF4 File Offset: 0x00000FF4
		public override bool HasKilled(string name)
		{
			global::PlayerData playerData = global::GameManager.instance.playerData;
			string boolName = "killed" + name;
			return playerData.GetBool(boolName);
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00002E24 File Offset: 0x00001024
		public override int KillsLeft(string name)
		{
			global::PlayerData playerData = global::GameManager.instance.playerData;
			string intName = "kills" + name;
			int num = playerData.GetInt(intName);
			bool flag = num < 0;
			if (flag)
			{
				num = 0;
			}
			return num;
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00002E64 File Offset: 0x00001064
		public override void RecordKillFor(string name)
		{
			global::PlayerData playerData = global::GameManager.instance.playerData;
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
					this.DisplayJournalUpdate(flag3);
				}
			}
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00002F80 File Offset: 0x00001180
		public override bool HasEntryFor(string name)
		{
			global::PlayerData playerData = global::GameManager.instance.playerData;
			string name2 = "killed" + name;
			return typeof(global::PlayerData).GetField(name2, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;
		}

		// Token: 0x04000007 RID: 7
		private static GameObject JournalUpdatePrefab;

		// Token: 0x04000008 RID: 8
		private static FieldInfo updateMessageInstance;

		// Token: 0x04000009 RID: 9
		private static FieldInfo getPrefabField;
	}
}
