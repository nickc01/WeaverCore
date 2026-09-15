using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore.Features
{
    /// <summary>
    /// Used for adding custom entries to SFCore's custom equipment inventory page.
    /// </summary>
    [ShowFeature]
    public abstract class InventoryItem : ScriptableObject
    {
        public enum ItemType
        {
            /// <summary>
            /// A single acquired/not-acquired item.
            /// </summary>
            Normal,

            /// <summary>
            /// An item with two mutually exclusive acquired states.
            /// </summary>
            OneTwo,

            /// <summary>
            /// An item with two acquired states that can also be shown together.
            /// </summary>
            OneTwoBoth,

            /// <summary>
            /// An item backed by an integer count. It displays when the count is greater than zero.
            /// </summary>
            Counted,

            /// <summary>
            /// A delicate-flower style item. It displays when acquired and not both from another source and broken.
            /// </summary>
            Flower
        }

        /// <summary>
        /// Stable unique ID used for generated PlayerData and language keys.
        /// </summary>
        public abstract string ItemID { get; }

        /// <summary>
        /// The inventory item behavior to register with SFCore.
        /// </summary>
        public virtual ItemType Type => ItemType.Normal;

        /// <summary>
        /// Main item sprite.
        /// </summary>
        public abstract Sprite Sprite { get; }

        /// <summary>
        /// Secondary sprite for OneTwo, OneTwoBoth, and Flower items.
        /// </summary>
        public virtual Sprite AlternateSprite => Sprite;

        /// <summary>
        /// Combined-state sprite for OneTwoBoth items.
        /// </summary>
        public virtual Sprite BothSprite => AlternateSprite;

        /// <summary>
        /// Main display name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Main description.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Secondary display name for OneTwo, OneTwoBoth, and Flower items.
        /// </summary>
        public virtual string AlternateName => Name;

        /// <summary>
        /// Secondary description for OneTwo, OneTwoBoth, and Flower items.
        /// </summary>
        public virtual string AlternateDescription => Description;

        /// <summary>
        /// Combined-state display name for OneTwoBoth items.
        /// </summary>
        public virtual string BothName => AlternateName;

        /// <summary>
        /// Combined-state description for OneTwoBoth items.
        /// </summary>
        public virtual string BothDescription => AlternateDescription;

        /// <summary>
        /// Flower description when the item is from another source but not broken.
        /// </summary>
        public virtual string FlowerOtherSourceDescription => Description;

        /// <summary>
        /// Flower description when the item is both from another source and broken.
        /// </summary>
        public virtual string FlowerBrokenOtherSourceDescription => AlternateDescription;

        /// <summary>
        /// Main acquired state for Normal, OneTwo, OneTwoBoth, and Flower items.
        /// </summary>
        public abstract bool Acquired { get; set; }

        /// <summary>
        /// Secondary acquired state for OneTwo and OneTwoBoth items.
        /// </summary>
        public virtual bool SecondAcquired { get; set; }

        /// <summary>
        /// Count for Counted items.
        /// </summary>
        public virtual int Count { get; set; }

        /// <summary>
        /// Whether a Flower item came from another source.
        /// </summary>
        public virtual bool FlowerFromOtherSource { get; set; }

        /// <summary>
        /// Whether a Flower item is broken.
        /// </summary>
        public virtual bool FlowerBroken { get; set; }
    }
}
