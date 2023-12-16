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
        public enum EntryType
        {
            Normal,
            Ghost,
            Grimm
        }

        /// <summary>
        /// The internal name of this journal entry
        /// </summary>
        public abstract string EntryName { get; }

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
        /// The image of the enemy shown in the hunter's journal when selected
        /// </summary>
        public abstract Sprite Sprite { get; }

        /// <summary>
        /// The icon shown in the menu at the left-side of the hunter's journal
        /// </summary>
        public abstract Sprite Icon { get; }

        /// <summary>
        /// The recorded amount of times this enemy has been killed. NOTE: This will stop counting up once the number reaches the <see cref="HuntersNotesThreshold"/>
        /// </summary>
        public abstract int KillCount { get; set; }

        /// <summary>
        /// Records whether or not this enemy has been discovered
        /// </summary>
        public abstract bool Discovered { get; set; }

        /// <summary>
        /// Is this a brand new entry in the journal? This is mainly used to highlight brand new elements in the journal
        /// </summary>
        public abstract bool IsNewEntry { get; set; }


        public virtual EntryType Type => EntryType.Normal;
	}
}
