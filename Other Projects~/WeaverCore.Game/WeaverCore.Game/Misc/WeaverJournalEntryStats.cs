using InControl;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Features;

namespace WeaverCore.Game
{
    public class WeaverJournalEntryStats : JournalEntryStats
	{
        const string NamePrefix = "NAME_";
        const string DescPrefix = "DESC_";
        const string NotesPrefix = "NOTE_";

        const string killCountPrefix = "kills";
        const string hasBeenKilledPrefix = "killed";
        const string newDataPrefix = "newData";

        static List<HunterJournalEntry> loadedEntries = new List<HunterJournalEntry>();
        static Dictionary<HunterJournalEntry, GameObject> builtJournalEntries = new Dictionary<HunterJournalEntry, GameObject>();

        static WeaverJournalEntryStats _prefab;

        public static WeaverJournalEntryStats CreatePrefab(JournalList list)
        {
            if (_prefab != null)
            {
                return _prefab;
            }

            var container = new GameObject("HUNTER_JOURNAL_ENTRY_CONTAINER");
            container.hideFlags = HideFlags.HideAndDontSave;
            container.SetActive(false);
            GameObject.DontDestroyOnLoad(container);

            var newPrefab = GameObject.Instantiate(list.list[0], container.transform);

            newPrefab.name = "Weaver Journal Entry Prefab";
            var old = newPrefab.GetComponent<JournalEntryStats>();
            var wjes = newPrefab.AddComponent<WeaverJournalEntryStats>();

            wjes.frameObject = old.frameObject;
            wjes.newDotObject = old.newDotObject;

            GameObject.DestroyImmediate(old);

            _prefab = wjes;

            return wjes;
        }

        [OnInit]
        static void Init()
        {
            On.JournalList.BuildEnemyList += JournalList_BuildEnemyList;
            On.JournalList.UpdateEnemyList += JournalList_UpdateEnemyList;

            On.JournalEntryStats.GetSprite += JournalEntryStats_GetSprite;
            On.JournalEntryStats.GetWarriorGhost += JournalEntryStats_GetWarriorGhost;
            On.JournalEntryStats.GetGrimm += JournalEntryStats_GetGrimm;
            ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;

            ModHooks.GetPlayerIntHook += ModHooks_GetPlayerIntHook;
            ModHooks.GetPlayerBoolHook += ModHooks_GetPlayerBoolHook;

            ModHooks.SetPlayerIntHook += ModHooks_SetPlayerIntHook;
            ModHooks.SetPlayerBoolHook += ModHooks_SetPlayerBoolHook;
        }

        private static void JournalList_UpdateEnemyList(On.JournalList.orig_UpdateEnemyList orig, JournalList self)
        {
            for (int i = 0; i < self.listInv.Length; i++)
            {
                if (self.listInv[i].TryGetComponent<WeaverJournalEntryStats>(out var wjes))
                {
                    wjes.Initialize(wjes.JournalEntry);
                }
            }

            orig(self);
        }

        private static void JournalList_BuildEnemyList(On.JournalList.orig_BuildEnemyList orig, JournalList self)
        {
            var prefab = CreatePrefab(self);

            builtJournalEntries.Clear();

            orig(self);

            var additionalCount = loadedEntries.Count;

            //var oldSize = self.listInv.Length;

            Array.Resize(ref self.listInv, self.listInv.Length + additionalCount);

            for (int i = 0; i < additionalCount; i++)
            {
                self.itemCount++;
                var wjes = GameObject.Instantiate(prefab);
                wjes.Initialize(loadedEntries[i]);
                var gameObject = wjes.gameObject;
                gameObject.transform.SetParent(self.transform, worldPositionStays: false);
                self.listInv[self.itemCount] = gameObject;

                builtJournalEntries.Add(loadedEntries[i], gameObject);
            }
        }

        private static bool JournalEntryStats_GetGrimm(On.JournalEntryStats.orig_GetGrimm orig, JournalEntryStats self)
        {
            if (self is WeaverJournalEntryStats wjes && wjes.JournalEntry != null)
            {
                return wjes.JournalEntry.Type == HunterJournalEntry.EntryType.Grimm;
            }
            return orig(self);
        }

        private static bool JournalEntryStats_GetWarriorGhost(On.JournalEntryStats.orig_GetWarriorGhost orig, JournalEntryStats self)
        {
            if (self is WeaverJournalEntryStats wjes && wjes.JournalEntry != null)
            {
                return wjes.JournalEntry.Type == HunterJournalEntry.EntryType.Ghost;
            }
            return orig(self);
        }

        private static Sprite JournalEntryStats_GetSprite(On.JournalEntryStats.orig_GetSprite orig, JournalEntryStats self)
        {
            if (self is WeaverJournalEntryStats wjes && wjes.JournalEntry != null)
            {
                return wjes.JournalEntry.Sprite;
            }
            return orig(self);
        }

        [OnFeatureLoad]
        static void EntryLoaded(HunterJournalEntry entry)
        {
            if (!loadedEntries.Contains(entry))
            {
                loadedEntries.Add(entry);
            }
        }

        private static bool ModHooks_GetPlayerBoolHook(string name, bool orig)
        {
            if (name.StartsWith(hasBeenKilledPrefix))
            {
                foreach (var entry in loadedEntries)
                {
                    if (name == hasBeenKilledPrefix + entry.EntryName)
                    {
                        return entry.Discovered;
                    }
                }
            }
            else if (name.StartsWith(newDataPrefix))
            {
                foreach (var entry in loadedEntries)
                {
                    if (name == newDataPrefix + entry.EntryName)
                    {
                        return entry.IsNewEntry;
                    }
                }
            }

            return orig;
        }

        private static int ModHooks_GetPlayerIntHook(string name, int orig)
        {
            if (name.StartsWith(killCountPrefix))
            {
                foreach (var entry in loadedEntries)
                {
                    if (name == killCountPrefix + entry.EntryName)
                    {
                        //NOTE: THIS MUST RETURN THE KILLS REMAINING TO COMPLETE THE ENTRY
                        return entry.HuntersNotesThreshold - entry.KillCount;
                    }
                }
            }

            return orig;
        }

        private static bool ModHooks_SetPlayerBoolHook(string name, bool orig)
        {
            if (name.StartsWith(hasBeenKilledPrefix))
            {
                foreach (var entry in loadedEntries)
                {
                    if (name == hasBeenKilledPrefix + entry.EntryName)
                    {
                        entry.Discovered = orig;
                        break;
                    }
                }
            }
            else if (name.StartsWith(newDataPrefix))
            {
                foreach (var entry in loadedEntries)
                {
                    if (name == newDataPrefix + entry.EntryName)
                    {
                        entry.IsNewEntry = orig;
                        break;
                    }
                }
            }

            return orig;
        }

        private static int ModHooks_SetPlayerIntHook(string name, int orig)
        {
            if (name.StartsWith(killCountPrefix))
            {
                foreach (var entry in loadedEntries)
                {
                    if (name == killCountPrefix + entry.EntryName)
                    {
                        //NOTE: ORIG REPRESENTS THE REMAINING KILLS LEFT
                        entry.KillCount = entry.HuntersNotesThreshold - orig;
                        break;
                    }
                }
            }

            return orig;
        }

        private static string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
        {
            if (sheetTitle == "Journal")
            {
                foreach (var entry in loadedEntries)
                {
                    if (key == NamePrefix + entry.EntryName.ToUpper() + "_CONVO")
                    {
                        return entry.Title;
                    }
                    else if (key == DescPrefix + entry.EntryName.ToUpper() + "_CONVO")
                    {
                        return entry.Description;
                    }
                    else if (key == NotesPrefix + entry.EntryName.ToUpper() + "_CONVO")
                    {
                        return entry.HuntersNotes;
                    }
                }
            }
            return orig;
        }

        [OnFeatureUnload]
        static void EntryUnloaded(HunterJournalEntry entry)
        {
            if (loadedEntries.Contains(entry))
            {
                loadedEntries.Remove(entry);
            }

            if (builtJournalEntries.TryGetValue(entry, out var entryObj) && entryObj != null && entryObj.transform.parent != null)
            {
                //TODO - Delete created object in the journal list
                RemoveLoadedJournalEntry(entryObj);
            }
        }

        static void RemoveLoadedJournalEntry(GameObject entryObj)
        {
            var journalList = entryObj.transform.parent.GetComponent<JournalList>();

            var listInvField = typeof(JournalList).GetField("listInv", BindingFlags.NonPublic | BindingFlags.Instance);

            var listInv = (GameObject[])listInvField.GetValue(journalList);

            if (listInv != null)
            {
                listInv = listInv.Where(g => g != entryObj).ToArray();
            }

            listInvField.SetValue(journalList, listInv);
        }

		public HunterJournalEntry JournalEntry { get; private set; }
		bool initialized = false;

		public void Initialize(HunterJournalEntry journalEntry)
		{
            if (initialized)
            {
                Uninitialize();
                initialized = false;
            }
            JournalEntry = journalEntry;

            gameObject.name = "Journal " + journalEntry.EntryName;

            playerDataName = journalEntry.EntryName;
            convoName = journalEntry.EntryName.ToUpper() + "_CONVO";

            nameConvo = NamePrefix + journalEntry.EntryName.ToUpper() + "_CONVO";
            descConvo = DescPrefix + journalEntry.EntryName.ToUpper() + "_CONVO";
            notesConvo = NotesPrefix + journalEntry.EntryName.ToUpper() + "_CONVO";

            playerDataKillsName = killCountPrefix + journalEntry.EntryName;
            playerDataBoolName = hasBeenKilledPrefix + journalEntry.EntryName;
            playerDataNewDataName = newDataPrefix + journalEntry.EntryName;

            warriorGhost = journalEntry.Type == HunterJournalEntry.EntryType.Ghost;
            grimmEntry = journalEntry.Type == HunterJournalEntry.EntryType.Grimm;

            transform.Find("Portrait").GetComponent<SpriteRenderer>().sprite = journalEntry.Icon;

            var nameObj = transform.Find("Name");

            nameObj.GetComponent<SetTextMeshProGameText>().convName = nameConvo;
            nameObj.GetComponent<TextMeshPro>().text = journalEntry.Title;
        }

        void Uninitialize()
		{

			JournalEntry = null;
		}
	}
}
