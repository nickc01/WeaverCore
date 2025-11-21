using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Modding;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Features;
using WeaverCore.Implementations;
using WeaverCore.Internal;
using WeaverCore.Utilities;

namespace WeaverCore
{
	public static class HunterJournal
	{
		[OnHarmonyPatch]
		static void OnHarmonyPatch(HarmonyPatcher patcher)
		{
			/*{
				var postfix = typeof(HunterJournal).GetMethod(nameof(GetPostfix), BindingFlags.NonPublic | BindingFlags.Static);
				foreach (var method in typeof(JournalList).GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name.Contains("Get") && m.DeclaringType == typeof(JournalList)))
				{
					WeaverLog.Log("Patching Method = " + method.Name);

					patcher.Patch(method, null, postfix);
				}
			}*/
			if (Initialization.Environment == Enums.RunningState.Editor)
			{
				return;
			}

			ModHooks.LanguageGetHook += CustomLanguageHook;
			ModHooks.GetPlayerIntHook += CustomIntGetterHook;
			ModHooks.GetPlayerBoolHook += CustomBoolGetterHook;
			ModHooks.SetPlayerIntHook += CustomIntSetterHook;
			ModHooks.SetPlayerBoolHook += CustomBoolSetterHook;

            {
				var orig = typeof(JournalList).GetMethod(nameof(JournalList.BuildEnemyList));
                var prefix = typeof(HunterJournal).GetMethod(nameof(BuildEnemyList_Prefix), BindingFlags.NonPublic | BindingFlags.Static);
				//var postfix = typeof(HunterJournal).GetMethod(nameof(BuildEnemyList_Postfix), BindingFlags.NonPublic | BindingFlags.Static);
				patcher.Patch(orig, prefix, null);
            }

            /*{
				var orig = typeof(JournalList).GetMethod(nameof(JournalList.UpdateEnemyList));
                var prefix = typeof(HunterJournal).GetMethod(nameof(UpdateEnemyList_Prefix), BindingFlags.NonPublic | BindingFlags.Static);
				patcher.Patch(orig, prefix, null);
            }*/



            /*{
				var orig = typeof(JournalEntryStats).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
                var postfix = typeof(HunterJournal).GetMethod(nameof(JournalEntryStats_Awake_Postfix), BindingFlags.NonPublic | BindingFlags.Static);
				patcher.Patch(orig, null, postfix);
            }*/

            /*{
                var orig = typeof(HeroController).Assembly.GetType("Language.Language").GetMethod("GetInternal", new Type[]{ typeof(string), typeof(string) });
				var prefix = typeof(HunterJournal).GetMethod(nameof(LanguageGetPrefix), BindingFlags.NonPublic | BindingFlags.Static);

				patcher.Patch(orig, prefix, null);
            }*/

            {
                var orig = typeof(PlayerData).GetMethod(nameof(PlayerData.CountJournalEntries));
				//var prefix = typeof(HunterJournal).GetMethod(nameof(HunterJournal.CountJournalEntries_Prefix), BindingFlags.NonPublic | BindingFlags.Static);
				var postfix = typeof(HunterJournal).GetMethod(nameof(HunterJournal.CountJournalEntries_Postfix), BindingFlags.NonPublic | BindingFlags.Static);

				patcher.Patch(orig, null, postfix);
            }

			{
				var orig = typeof(GameManager).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(m => m.Name == "SaveGame" && m.GetParameters().Length >= 2);
				var prefix = typeof(HunterJournal).GetMethod(nameof(SaveGame_Prefix), BindingFlags.NonPublic | BindingFlags.Static);

				patcher.Patch(orig, prefix, null);
			}
		}

		static bool SaveGame_Prefix(GameManager __instance, int saveSlot, ref Action<bool> callback)
		{
			int prevValue = PlayerData.instance.GetInt("lastJournalItem");
			var oldCallback = callback;
			callback = didSave =>
			{
				try
				{
					//After Game Saved
					PlayerData.instance.SetInt("lastJournalItem", prevValue);
				}
				finally
				{
					oldCallback?.Invoke(didSave);
				}
			};

			PlayerData.instance.SetInt("lastJournalItem", 0);

			//Before Game Saved

			return true;
		}

		/*static void GetPostfix(MethodBase __originalMethod, object[] __args, object __result)
        {
			string args = "{ ";
			foreach (var arg in __args)
			{
				args += $"{arg}, ";
			}

			args += "}";
            WeaverLog.Log($"Calling {__originalMethod.Name} with args {args} with result {__result}");
        }*/

		static bool LanguageGetPrefix(string key, string sheetTitle)
        {
            return true;
        }
		
		static string CustomLanguageHook(string key, string sheetTitle, string orig)
        {
		if (string.IsNullOrEmpty(key))
		{
			WeaverLog.LogWarning($"CustomLanguageHook received null or empty key! SheetTitle: {sheetTitle}");
			return orig ?? "";
		}

            if (sheetTitle == "Journal")
			{
				if (key.StartsWith("NAME_"))
				{
					var entryName = key.Substring("NAME_".Length);
					if (nameToEntryMapping.TryGetValue(entryName, out var hunter))
					{
						WeaverLog.Log($"CustomLanguageHook: Returning Title for '{entryName}': {hunter.Title}");
						return hunter.Title;
					}
				}

				if (key.StartsWith("DESC_"))
				{
					var entryName = key.Substring("DESC_".Length);
					if (nameToEntryMapping.TryGetValue(entryName, out var hunter))
					{
						WeaverLog.Log($"CustomLanguageHook: Returning Description for '{entryName}'");
						return hunter.Description;
					}
				}

				if (key.StartsWith("NOTE_"))
				{
					var entryName = key.Substring("NOTE_".Length);
					if (nameToEntryMapping.TryGetValue(entryName, out var hunter))
					{
						WeaverLog.Log($"CustomLanguageHook: Returning HuntersNotes for '{entryName}'");
						return hunter.HuntersNotes;
					}
				}
			}

			return orig;
        }

		static int CustomIntGetterHook(string name, int orig)
        {
            if (name.StartsWith("kills"))
			{
				if (nameToEntryMapping.TryGetValue(name.Substring("kills".Length), out var hunter))
				{
					return Mathf.Clamp(hunter.HuntersNotesThreshold - hunter.KillCount, 0, 10000);
				}
			}

			return orig;
        }

		static bool CustomBoolGetterHook(string name, bool orig)
        {
			if (name.StartsWith("killed"))
			{
				if (nameToEntryMapping.TryGetValue(name.Substring("killed".Length), out var hunter))
				{
					return hunter.Discovered;
				}
			}

			if (name.StartsWith("newData"))
			{
				if (nameToEntryMapping.TryGetValue(name.Substring("newData".Length), out var hunter))
				{
					return hunter.IsNewEntry;
				}
			}

			return orig;
        }

		static int CustomIntSetterHook(string name, int orig)
        {
            if (name.StartsWith("kills"))
			{
				if (nameToEntryMapping.TryGetValue(name.Substring("kills".Length), out var hunter))
				{
					hunter.KillCount = hunter.HuntersNotesThreshold - orig;

					return orig;
				}
			}

			return orig;
        }

		static bool CustomBoolSetterHook(string name, bool orig)
        {
			if (name.StartsWith("killed"))
			{
				if (nameToEntryMapping.TryGetValue(name.Substring("killed".Length), out var hunter))
				{
					hunter.Discovered = orig;
					return orig;
				}
			}

			if (name.StartsWith("newData"))
			{
				if (nameToEntryMapping.TryGetValue(name.Substring("newData".Length), out var hunter))
				{
					hunter.IsNewEntry = orig;
					return orig;
				}
			}

			return orig;
        }


		static GameObject __hunterJournalPrefabContainer;

		static GameObject HunterJournalPrefabContainer
        {
			get
            {
				if (__hunterJournalPrefabContainer == null)
				{
					var c = new GameObject("__CUSTOM WEAVER JOURNAL ENTRIES__");
					GameObject.DontDestroyOnLoad(c);
					c.gameObject.SetActive(false);
					c.hideFlags = HideFlags.HideAndDontSave;
					__hunterJournalPrefabContainer = c;
				}
				return __hunterJournalPrefabContainer;
            }
        }

		public static bool HasKilled(string name)
        {
            return PlayerData.instance.GetBool($"killed{name}");
        }

		public static bool HasKilled(HunterJournalEntry entry) => HasKilled(entry.EntryName);

		public static int KillsLeft(string name)
        {
            return PlayerData.instance.GetInt($"kills{name}");
        }

		public static int KillsLeft(HunterJournalEntry entry) => KillsLeft(entry.EntryName);

		public static void RecordKillFor(string name)
        {
			//WeaverLog.Log("RECORDING KILL FOR " + name);
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

		public static void RecordKillFor(HunterJournalEntry entry) => RecordKillFor(entry.EntryName);

		static JournalList _journal;

		public static bool HasEntryFor(string name)
        {
			if (Initialization.Environment == Enums.RunningState.Editor)
			{
				return false;
			}
			
			if (_journal == null)
			{
				_journal = GameObject.FindObjectOfType<JournalList>(true);
			}

            foreach (var entry in _journal.list.Select(j => j.GetComponent<JournalEntryStats>()))
			{
				if (entry.playerDataName == name)
				{
					return true;
				}
			}

			foreach (var entry in Registry.GetAllFeatures<HunterJournalEntry>())
            {
                if (entry.EntryName == name)
				{
					return true;
				}
            }

			return false;
        }

		private static FieldInfo updateMessageInstance;

		public static bool HasEntryFor(HunterJournalEntry entry) => HasEntryFor(entry.EntryName);

		private static GameObject SpawnJournalUpdate()
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(Other_Preloads.JournalUpdateMessagePrefab);
            gameObject.SetActive(false);
            return gameObject;
        }

		public static void DisplayJournalUpdate(bool displayText = false)
        {
            if (updateMessageInstance == null)
            {
                updateMessageInstance = typeof(HeroController).Assembly.GetType("EnemyDeathEffects").GetField("journalUpdateMessageSpawned", BindingFlags.Static | BindingFlags.NonPublic);
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

			var fsm = PlayMakerUtilities.FindPlayMakerFSMWrapper(gameObject, "Journal Msg");

			if (fsm != default)
			{
				fsm.GetFsm().SetBoolVariable("Full", displayText);
				fsm.GetFsm().SetBoolVariable("Should Recycle", true);
			}
        }

		static Dictionary<HunterJournalEntry, GameObject> createdPrefabs = new Dictionary<HunterJournalEntry, GameObject>();
		static Dictionary<string, HunterJournalEntry> nameToEntryMapping = new Dictionary<string, HunterJournalEntry>();

		static bool BuildEnemyList_Prefix(JournalList __instance)
		{
			List<GameObject> newEntries = null;
			var prefab = __instance.list.First(g => g != null);
			var allEntries = Registry.GetAllFeatures<HunterJournalEntry>().ToList();

			var setGameTextType = typeof(HeroController).Assembly.GetType("SetTextMeshProGameText");

			foreach (var entry in allEntries)
			{
				if (!createdPrefabs.ContainsKey(entry))
				{
					var customEntryPrefab = GameObject.Instantiate(prefab, HunterJournalPrefabContainer.transform);
					customEntryPrefab.SetActive(true);
					customEntryPrefab.name = $"Journal {entry.EntryName}";
					createdPrefabs.Add(entry, customEntryPrefab);

					var entryName = entry.EntryName;

					var journalList = customEntryPrefab.GetComponent<JournalEntryStats>();
					journalList.playerDataName = entryName.ToString();
					journalList.convoName = entryName.ToString();
					journalList.sprite = entry.Sprite;

					var setGameText = journalList.GetComponentInChildren(setGameTextType) as MonoBehaviour;
					if (setGameText != null)
					{
						setGameText.ReflectSetField("convName", $"NAME_{entry.EntryName}");
					}
					var entryType = entry.Type;
					journalList.warriorGhost = entryType == HunterJournalEntry.EntryType.Ghost;
					journalList.grimmEntry = entryType == HunterJournalEntry.EntryType.Grimm;
					journalList.transform.Find("Portrait").GetComponent<SpriteRenderer>().sprite = entry.Icon;
					nameToEntryMapping.Add(entryName.ToString(), entry);

					if (newEntries == null)
					{
						newEntries = new List<GameObject>();
					}
					newEntries.Add(customEntryPrefab);
				}
			}

			if (newEntries != null)
			{
				//WeaverLog.Log("OLD size = " + __instance.list.Length);
				//WeaverLog.Log("Entry Count = " + newEntries.Count);
				Array.Resize(ref __instance.list, __instance.list.Length + newEntries.Count);
				//WeaverLog.Log("NEW size = " + __instance.list.Length);

				for (int i = 0; i < newEntries.Count; i++)
				{
					__instance.list[__instance.list.Length - (newEntries.Count - i)] = newEntries[i];
				}
				/*for (int i = __instance.list.Length - newEntries.Count - 1; i < __instance.list.Length; i++)
				{
					WeaverLog.Log("I = " + i);
					WeaverLog.Log("Entry Index = " + (i - (__instance.list.Length - newEntries.Count - 1)));
					__instance.list[i] = newEntries[i - (__instance.list.Length - newEntries.Count - 1)];
				}*/
			}

			return true;
        }

		/*static void BuildEnemyList_Postfix(JournalList __instance)
        {
            //WeaverLog.Log("Item Count = " + __instance.itemCount);
        }*/

		static void CountJournalEntries_Postfix(PlayerData __instance)
        {
			var total = PlayerData.instance.GetInt("journalEntriesTotal");
			var notesCompleted = PlayerData.instance.GetInt("journalNotesCompleted");
			var entriesCompleted = PlayerData.instance.GetInt("journalEntriesCompleted");
            foreach (var entry in Registry.GetAllFeatures<HunterJournalEntry>())
			{
				//WeaverLog.Log("Counting Entry = " + entry.EntryName);
				total++;
				if (entry.KillCount >= entry.HuntersNotesThreshold)
				{
					notesCompleted++;
				}

				if (entry.Discovered)
				{
					entriesCompleted++;
				}
			}

			PlayerData.instance.SetInt("journalEntriesTotal", total);
			PlayerData.instance.SetInt("journalNotesCompleted", notesCompleted);
			PlayerData.instance.SetInt("journalEntriesCompleted", entriesCompleted);

			if (PlayerData.instance.GetInt("lastJournalItem") > PlayerData.instance.GetInt("journalEntriesCompleted") - 1)
			{
				PlayerData.instance.SetInt("lastJournalItem", PlayerData.instance.GetInt("journalEntriesCompleted") - 1);
			}
        }
    }

	/// <summary>
	/// Used for recording entries into the hunter's journal
	/// </summary>
    /*public static class HunterJournalOLD
	{
		/// <summary>
		/// Has the player killed this enemy at least once?
		/// </summary>
		/// <param name="name">The name of the enemy in the hunter's journal</param>
		/// <returns>Returns whether this player has killed the enemy before</returns>
		public static bool HasKilled(string name)
		{
			return HunterJournal.impl.HasKilled(name);
		}

		/// <summary>
		/// How many kills are left to fully unlock the enemy?
		/// </summary>
		/// <param name="name">The name of the enemy in the hunter's journal</param>
		/// <returns>Returns how many kills are left to fully unlock the enemy</returns>
		public static int KillsLeft(string name)
		{
			return HunterJournal.impl.KillsLeft(name);

		}

		/// <summary>
		/// Records a kill for the enemy
		/// </summary>
		/// <param name="name">The name of the enemy in the hunter's journal</param>
		public static void RecordKillFor(string name)
		{
			HunterJournal.impl.RecordKillFor(name);
		}

		/// <summary>
		/// Does the enemy entry exist in the hunter's journal? 
		/// </summary>
		/// <param name="name">The name of the enemy in the hunter's journal</param>
		/// <returns>Returns whether the enemy entry exists in the hunter's journal</returns>
		public static bool HasEntryFor(string name)
		{
			return HunterJournal.impl.HasEntryFor(name);
		}






        /// <summary>
        /// Has the player killed this enemy at least once?
        /// </summary>
        /// <param name="entry">The entry of the enemy in the hunter's journal</param>
        /// <returns>Returns whether this player has killed the enemy before</returns>
        public static bool HasKilled(HunterJournalEntry entry)
        {
            return HunterJournal.impl.HasKilled(entry.EntryName);
        }

        /// <summary>
        /// How many kills are left to fully unlock the enemy?
        /// </summary>
        /// <param name="name">The entry of the enemy in the hunter's journal</param>
        /// <returns>Returns how many kills are left to fully unlock the enemy</returns>
        public static int KillsLeft(HunterJournalEntry entry)
        {
            return HunterJournal.impl.KillsLeft(entry.EntryName);
        }

        /// <summary>
        /// Records a kill for the enemy
        /// </summary>
        /// <param name="name">The entry of the enemy in the hunter's journal</param>
        public static void RecordKillFor(HunterJournalEntry entry)
        {
            HunterJournal.impl.RecordKillFor(entry.EntryName);
        }

        /// <summary>
        /// Does the enemy entry exist in the hunter's journal?
        /// </summary>
        /// <param name="name">The entry of the enemy in the hunter's journal</param>
        /// <returns>Returns whether the enemy entry exists in the hunter's journal</returns>
        public static bool HasEntryFor(HunterJournalEntry entry)
        {
            return HunterJournal.impl.HasEntryFor(entry.EntryName);
        }

        /// <summary>
        /// Displays an icon at the bottom-right of the screen indicating the hunter's journal was updated
        /// </summary>
        /// <param name="displayText">Should the text "Journal Updated" also be displayed?</param>
        public static void DisplayJournalUpdate(bool displayText = false)
		{
			HunterJournal.impl.DisplayJournalUpdate(displayText);
		}

		private static HunterJournal_I impl = ImplFinder.GetImplementation<HunterJournal_I>();
	}*/
}
