using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore.Features
{
    /// <summary>
    /// Used for adding new hunter's journal entries
    /// </summary>
    [ShowFeature]
    public abstract class HunterJournalEntry : ScriptableObject
	{
        /// <summary>
        /// The title of the enemy in the hunter's journal
        /// </summary>
        public abstract string Title { get; }

        /// <summary>
        /// The description of the enemy in the hunter's journal
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// The amount of kills required before the <see cref="HuntersNotes"/> becomes visible
        /// </summary>
        public abstract int HuntersNotesThreshold { get; }

        /// <summary>
        /// The hunter's notes on the enemy in the hunter's journal. Only visible after the amount of kills has reached the <see cref="HuntersNotesThreshold"/>
        /// </summary>
        public abstract string HuntersNotes { get; }

        /// <summary>
        /// The image of the enemy shown in the hunter's journal
        /// </summary>
        public abstract Sprite Sprite { get; }
	}

    /// <summary>
    /// A basic hunter's journal entry that uses basic strings
    /// </summary>
    public class BasicHunterJournalEntry : HunterJournalEntry
    {
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
        [Tooltip("The image of the enemy shown in the hunter's journal")]
        protected Sprite sprite;

        public override string Title => title;

        public override string Description => description;

        public override int HuntersNotesThreshold => huntersNotesThreshold;

        public override string HuntersNotes => huntersNotes;

        public override Sprite Sprite => sprite;
    }

    /// <summary>
    /// A more advanced hunter's journal entry that uses translation keys
    /// </summary>
    public class AdvancedHunterJournalEntry : HunterJournalEntry
    {
        [SerializeField]
        [Tooltip("The title of the enemy in the hunter's journal")]
        protected string titleKey;

        [SerializeField]
        [Tooltip("The title of the enemy in the hunter's journal")]
        protected string titleSheetTitle;

        [SerializeField]
        [Tooltip("The description of the enemy in the hunter's journal")]
        protected string descriptionKey;

        [SerializeField]
        [Tooltip("The description of the enemy in the hunter's journal")]
        protected string descriptionSheetTitle;

        [SerializeField]
        [Tooltip("The amount of kills required before the Hunter's Notes becomes visible")]
        protected int huntersNotesThreshold;

        [SerializeField]
        [Tooltip("The hunter's notes on the enemy in the hunter's journal. Only visible after the amount of kills has reached the Hunter's Notes Threshold")]
        protected string huntersNotesKey;

        [SerializeField]
        [Tooltip("The hunter's notes on the enemy in the hunter's journal. Only visible after the amount of kills has reached the Hunter's Notes Threshold")]
        protected string huntersNotesSheetTitle;

        [SerializeField]
        [Tooltip("The image of the enemy shown in the hunter's journal")]
        protected Sprite sprite;

        public override string Title => WeaverLanguage.GetString(titleKey, titleSheetTitle, "BROKEN_TITLE");

        public override string Description => WeaverLanguage.GetString(descriptionKey, descriptionSheetTitle, "BROKEN_DESCRIPTION");

        public override int HuntersNotesThreshold => huntersNotesThreshold;

        public override string HuntersNotes => WeaverLanguage.GetString(huntersNotesKey, huntersNotesSheetTitle, "BROKEN_HUNTERS_NOTES");

        public override Sprite Sprite => sprite;
    }
}
