using Newtonsoft.Json.Linq;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Settings;

namespace WeaverCore.Features
{
    /// <summary>
    /// A basic hunter's journal entry that uses basic strings
    /// </summary>
    [CreateAssetMenu(fileName = "Basic Hunter Journal Entry", menuName = "WeaverCore/Basic Hunter Journal Entry")]
    public class BasicHunterJournalEntry : HunterJournalEntry
    {
        [SerializeField]
        [Tooltip("The internal name of this journal entry")]
        protected string entryName;

        [SerializeField]
        [Tooltip("The title of the enemy in the hunter's journal")]
        protected string title;

        [SerializeField]
        [Tooltip("The description of the enemy in the hunter's journal")]
        protected string description;

        [SerializeField]
        [Tooltip("The amount of kills required before the Hunter's Notes becomes visible")]
        protected int huntersNotesThreshold;

        [SerializeField]
        [Tooltip("The hunter's notes on the enemy in the hunter's journal. Only visible after the amount of kills has reached the Hunter's Notes Threshold")]
        protected string huntersNotes;

        [SerializeField]
        [Tooltip("The image of the enemy shown in the hunter's journal when selected")]
        protected Sprite sprite;

        [SerializeField]
        [Tooltip("The icon shown in the menu at the left-side of the hunter's journal")]
        protected Sprite icon;

        [Header("Persistent Settings")]
        [SerializeField]
        [Tooltip("Used for storing how many times this enemy was killed, and whether or not it has been discovered")]
        protected SaveSpecificSettings saveSettings;

        [SerializeField]
        [Tooltip("The field in the Save Settings used for storing the kill count")]
        [SaveSpecificFieldName(typeof(int), nameof(saveSettings))]
        protected string killCountSaveFieldName;

        [SerializeField]
        [Tooltip("The field in the Save Settings used for storing whether or not the enemy has been discovered")]
        [SaveSpecificFieldName(typeof(bool), nameof(saveSettings))]
        protected string discoveredSaveFieldName;

        [SerializeField]
        [Tooltip("The field in the Save Settings used for storing whether or not this is a brand new entry in the journal")]
        [SaveSpecificFieldName(typeof(bool), nameof(saveSettings))]
        protected string isNewEntrySaveFieldName;

        public override string EntryName => entryName;

        public override string Title => title;

        public override string Description => description;

        public override int HuntersNotesThreshold => huntersNotesThreshold;

        public override string HuntersNotes => huntersNotes;

        public override Sprite Sprite => sprite;

        public override Sprite Icon => icon;

        int _killCount_internal;
        public override int KillCount
        {
            get
            {
                if (saveSettings.TryGetFieldValue<int>(killCountSaveFieldName, out var result))
                {
                    return result;
                }
                else
                {
                    WeaverLog.LogError($"Error: {killCountSaveFieldName} is not a valid field in {saveSettings.GetType().FullName}. KillCount will not be saved");
                    return _killCount_internal;
                }
            }
            set
            {
                if (saveSettings.HasField<int>(killCountSaveFieldName))
                {
                    saveSettings.SetFieldValue(killCountSaveFieldName, value);
                }
                else
                {
                    WeaverLog.LogError($"Error: {killCountSaveFieldName} is not a valid field in {saveSettings.GetType().FullName}. \"KillCount\" will not be saved");
                    _killCount_internal = value;
                }
            }
        }

        bool discovered_internal;
        public override bool Discovered
        {
            get
            {
                return true;
                if (saveSettings.TryGetFieldValue<bool>(discoveredSaveFieldName, out var result))
                {
                    return result;
                }
                else
                {
                    WeaverLog.LogError($"Error: {discoveredSaveFieldName} is not a valid field in {saveSettings.GetType().FullName}. \"Discovered\" will not be saved");
                    return discovered_internal;
                }
            }
            set
            {
                if (saveSettings.HasField<bool>(discoveredSaveFieldName))
                {
                    saveSettings.SetFieldValue(discoveredSaveFieldName, value);
                }
                else
                {
                    WeaverLog.LogError($"Error: {discoveredSaveFieldName} is not a valid field in {saveSettings.GetType().FullName}. \"Discovered\" will not be saved");
                    discovered_internal = value;
                }
            }
        }

        bool _isNewEntry_internal;
        public override bool IsNewEntry
        {
            get
            {
                if (saveSettings.TryGetFieldValue<bool>(isNewEntrySaveFieldName, out var result))
                {
                    return result;
                }
                else
                {
                    WeaverLog.LogError($"Error: {isNewEntrySaveFieldName} is not a valid field in {saveSettings.GetType().FullName}. \"IsNewEntry\" will not be saved");
                    return _isNewEntry_internal;
                }
            }
            set
            {
                if (saveSettings.HasField<bool>(isNewEntrySaveFieldName))
                {
                    saveSettings.SetFieldValue(isNewEntrySaveFieldName, value);
                }
                else
                {
                    WeaverLog.LogError($"Error: {isNewEntrySaveFieldName} is not a valid field in {saveSettings.GetType().FullName}. \"IsNewEntry\" will not be saved");
                    _isNewEntry_internal = value;
                }
            }
        }
    }
}
