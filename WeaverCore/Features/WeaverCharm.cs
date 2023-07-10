using System;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Interfaces;
using WeaverCore.Settings;
using WeaverCore.Utilities;

namespace WeaverCore.Features
{
    [ShowFeature]
    [CreateAssetMenu(fileName = "WeaverCharm", menuName = "WeaverCore/Weaver Charm")]
    public abstract class WeaverCharm : ScriptableObject, IWeaverCharm
    {
        [field: SerializeField]
        public bool AcquiredByDefault { get; set; } = true;

        [SerializeField]
        [Tooltip("The save specific settings that will persistently store settings about the charm")]
        SaveSpecificSettings settingsStorage;

        [SerializeField]
        [Tooltip("The name of a boolean field on the Settings Storage for storing whether or not the charm has been acquired")]
        string acquired_settingsField;

        [SerializeField]
        [Tooltip("The name of a boolean field on the Settings Storage for storing whether or not the charm has been equipped")]
        string equipped_settingsField;

        [SerializeField]
        [Tooltip("The name of a boolean field on the Settings Storage for storing whether or not the charm has been newly collected")]
        string newlyCollected_settingsField;

        [field: SerializeField]
        public Sprite CharmSprite { get; private set; }

        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract int NotchCost { get; }

        public virtual bool Acquired
        {
            get
            {
                if (AcquiredByDefault)
                {
                    return true;
                }
                if (settingsStorage.HasField(acquired_settingsField))
                {
                    return settingsStorage.GetFieldValue<bool>(acquired_settingsField);
                }
                else
                {
                    throw new Exception($"The settings field {StringUtilities.Prettify(nameof(acquired_settingsField))} doesn't point to a valid field");
                }
            }

            set
            {
                if (settingsStorage.HasField(acquired_settingsField))
                {
                    settingsStorage.SetFieldValue(acquired_settingsField, value);
                }
                else
                {
                    throw new Exception($"The settings field {StringUtilities.Prettify(nameof(acquired_settingsField))} doesn't point to a valid field");
                }
            }
        }
        public virtual bool Equipped
        {
            get
            {
                if (settingsStorage.HasField(equipped_settingsField))
                {
                    return settingsStorage.GetFieldValue<bool>(equipped_settingsField);
                }
                else
                {
                    throw new Exception($"The settings field {StringUtilities.Prettify(nameof(equipped_settingsField))} doesn't point to a valid field");
                }
            }

            set
            {
                if (settingsStorage.HasField(equipped_settingsField))
                {
                    settingsStorage.SetFieldValue(equipped_settingsField, value);
                }
                else
                {
                    throw new Exception($"The settings field {StringUtilities.Prettify(nameof(equipped_settingsField))} doesn't point to a valid field");
                }
            }
        }
        public virtual bool NewlyCollected
        {
            get
            {
                if (settingsStorage.HasField(newlyCollected_settingsField))
                {
                    return settingsStorage.GetFieldValue<bool>(newlyCollected_settingsField);
                }
                else
                {
                    throw new Exception($"The settings field {StringUtilities.Prettify(nameof(newlyCollected_settingsField))} doesn't point to a valid field");
                }
            }

            set
            {
                if (settingsStorage.HasField(newlyCollected_settingsField))
                {
                    settingsStorage.SetFieldValue(newlyCollected_settingsField, value);
                }
                else
                {
                    throw new Exception($"The settings field {StringUtilities.Prettify(nameof(newlyCollected_settingsField))} doesn't point to a valid field");
                }
            }
        }
    }
}
