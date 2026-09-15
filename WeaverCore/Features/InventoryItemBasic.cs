using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Settings;

namespace WeaverCore.Features
{
    /// <summary>
    /// A basic inventory item that can be configured from the editor.
    /// </summary>
    [CreateAssetMenu(fileName = "Inventory Item Basic", menuName = "WeaverCore/Inventory Item Basic")]
    public class InventoryItemBasic : InventoryItem
    {
        [SerializeField]
        [Tooltip("Stable unique ID used for generated PlayerData and language keys. Keep this value consistent between releases.")]
        protected string itemID;

        [SerializeField]
        [Tooltip("The inventory item behavior to register with SFCore.")]
        protected ItemType itemType;

        [SerializeField]
        [Tooltip("Main item sprite.")]
        protected Sprite sprite;

        [SerializeField]
        [Tooltip("Secondary sprite for OneTwo, OneTwoBoth, and Flower items.")]
        protected Sprite alternateSprite;

        [SerializeField]
        [Tooltip("Combined-state sprite for OneTwoBoth items.")]
        protected Sprite bothSprite;

        [SerializeField]
        [Tooltip("Main display name.")]
        protected string itemName;

        [SerializeField]
        [TextArea]
        [Tooltip("Main description.")]
        protected string description;

        [SerializeField]
        [Tooltip("Secondary display name for OneTwo, OneTwoBoth, and Flower items.")]
        protected string alternateName;

        [SerializeField]
        [TextArea]
        [Tooltip("Secondary description for OneTwo, OneTwoBoth, and Flower items.")]
        protected string alternateDescription;

        [SerializeField]
        [Tooltip("Combined-state display name for OneTwoBoth items.")]
        protected string bothName;

        [SerializeField]
        [TextArea]
        [Tooltip("Combined-state description for OneTwoBoth items.")]
        protected string bothDescription;

        [SerializeField]
        [TextArea]
        [Tooltip("Flower description when the item is from another source but not broken.")]
        protected string flowerOtherSourceDescription;

        [SerializeField]
        [TextArea]
        [Tooltip("Flower description when the item is both from another source and broken.")]
        protected string flowerBrokenOtherSourceDescription;

        [Header("Default State")]
        [SerializeField]
        protected bool acquiredByDefault;

        [SerializeField]
        protected bool secondAcquiredByDefault;

        [SerializeField]
        protected int countByDefault;

        [SerializeField]
        protected bool flowerFromOtherSourceByDefault;

        [SerializeField]
        protected bool flowerBrokenByDefault;

        [Header("Persistent Settings")]
        [SerializeField]
        [Tooltip("Used for storing this item's acquired state, count, or flower flags.")]
        protected SaveSpecificSettings saveSettings;

        [SerializeField]
        [Tooltip("The field in Save Settings used for storing whether this item has been acquired.")]
        [SaveSpecificFieldName(typeof(bool), nameof(saveSettings))]
        protected string acquiredSaveFieldName;

        [SerializeField]
        [Tooltip("The field in Save Settings used for storing the secondary acquired state.")]
        [SaveSpecificFieldName(typeof(bool), nameof(saveSettings))]
        protected string secondAcquiredSaveFieldName;

        [SerializeField]
        [Tooltip("The field in Save Settings used for storing the item count.")]
        [SaveSpecificFieldName(typeof(int), nameof(saveSettings))]
        protected string countSaveFieldName;

        [SerializeField]
        [Tooltip("The field in Save Settings used for storing whether this flower item is from another source.")]
        [SaveSpecificFieldName(typeof(bool), nameof(saveSettings))]
        protected string flowerFromOtherSourceSaveFieldName;

        [SerializeField]
        [Tooltip("The field in Save Settings used for storing whether this flower item is broken.")]
        [SaveSpecificFieldName(typeof(bool), nameof(saveSettings))]
        protected string flowerBrokenSaveFieldName;

        bool settingsFixed;
        bool acquiredInternal;
        bool secondAcquiredInternal;
        bool flowerFromOtherSourceInternal;
        bool flowerBrokenInternal;
        int countInternal;

        SaveSpecificSettings GetSettings()
        {
            if (!settingsFixed)
            {
                settingsFixed = true;
                acquiredInternal = acquiredByDefault;
                secondAcquiredInternal = secondAcquiredByDefault;
                countInternal = countByDefault;
                flowerFromOtherSourceInternal = flowerFromOtherSourceByDefault;
                flowerBrokenInternal = flowerBrokenByDefault;

                if (saveSettings != null)
                {
                    saveSettings = SaveSpecificSettings.GetSaveSettings(saveSettings.GetType());
                }
            }

            return saveSettings;
        }

        bool GetBool(string fieldName, bool fallback)
        {
            if (GetSettings().TryGetFieldValue<bool>(fieldName, out var result))
            {
                return result;
            }

            return fallback;
        }

        void SetBool(string fieldName, bool value, ref bool fallback)
        {
            if (!GetSettings().TrySetFieldValue(fieldName, value))
            {
                fallback = value;
            }
        }

        int GetInt(string fieldName, int fallback)
        {
            if (GetSettings().TryGetFieldValue<int>(fieldName, out var result))
            {
                return result;
            }

            return fallback;
        }

        void SetInt(string fieldName, int value, ref int fallback)
        {
            if (!GetSettings().TrySetFieldValue(fieldName, value))
            {
                fallback = value;
            }
        }

        public override string ItemID => itemID;

        public override ItemType Type => itemType;

        public override Sprite Sprite => sprite;

        public override Sprite AlternateSprite => alternateSprite != null ? alternateSprite : Sprite;

        public override Sprite BothSprite => bothSprite != null ? bothSprite : AlternateSprite;

        public override string Name => itemName;

        public override string Description => description;

        public override string AlternateName => string.IsNullOrEmpty(alternateName) ? Name : alternateName;

        public override string AlternateDescription => string.IsNullOrEmpty(alternateDescription) ? Description : alternateDescription;

        public override string BothName => string.IsNullOrEmpty(bothName) ? AlternateName : bothName;

        public override string BothDescription => string.IsNullOrEmpty(bothDescription) ? AlternateDescription : bothDescription;

        public override string FlowerOtherSourceDescription => string.IsNullOrEmpty(flowerOtherSourceDescription) ? Description : flowerOtherSourceDescription;

        public override string FlowerBrokenOtherSourceDescription => string.IsNullOrEmpty(flowerBrokenOtherSourceDescription) ? AlternateDescription : flowerBrokenOtherSourceDescription;

        public override bool Acquired
        {
            get => GetBool(acquiredSaveFieldName, acquiredInternal);
            set => SetBool(acquiredSaveFieldName, value, ref acquiredInternal);
        }

        public override bool SecondAcquired
        {
            get => GetBool(secondAcquiredSaveFieldName, secondAcquiredInternal);
            set => SetBool(secondAcquiredSaveFieldName, value, ref secondAcquiredInternal);
        }

        public override int Count
        {
            get => GetInt(countSaveFieldName, countInternal);
            set => SetInt(countSaveFieldName, value, ref countInternal);
        }

        public override bool FlowerFromOtherSource
        {
            get => GetBool(flowerFromOtherSourceSaveFieldName, flowerFromOtherSourceInternal);
            set => SetBool(flowerFromOtherSourceSaveFieldName, value, ref flowerFromOtherSourceInternal);
        }

        public override bool FlowerBroken
        {
            get => GetBool(flowerBrokenSaveFieldName, flowerBrokenInternal);
            set => SetBool(flowerBrokenSaveFieldName, value, ref flowerBrokenInternal);
        }
    }
}
